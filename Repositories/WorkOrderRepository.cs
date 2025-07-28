using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// WorkOrder Repository 구현체
    /// </summary>
    public class WorkOrderRepository : BaseRepository<WorkOrder>, IWorkOrderRepository
    {
        public WorkOrderRepository(MesDbContext context) : base(context)
        {
        }

        // 고유 번호로 조회
        public async Task<WorkOrder?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _dbSet
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .FirstOrDefaultAsync(w => w.OrderNumber == orderNumber);
        }

        // 상세 정보 포함 조회
        public async Task<WorkOrder?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Pallet)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.CurrentLocation)
                .Include(w => w.Recipe)
                    .ThenInclude(r => r.Steps)
                .Include(w => w.ProcessExecutions)
                    .ThenInclude(e => e.RecipeStep)
                .Include(w => w.Logs)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WorkOrder?> GetWithExecutionsAsync(Guid id)
        {
            return await _dbSet
                .Include(w => w.ProcessExecutions)
                    .ThenInclude(e => e.RecipeStep)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        // 상태별 조회
        public async Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status)
        {
            return await _dbSet
                .Where(w => w.Status == status)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrder>> GetActiveOrdersAsync()
        {
            var activeStatuses = new[] {
                WorkOrderStatus.Created,
                WorkOrderStatus.Scheduled,
                WorkOrderStatus.InProgress
            };

            return await _dbSet
                .Where(w => activeStatuses.Contains(w.Status))
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrder>> GetPendingOrdersAsync()
        {
            var pendingStatuses = new[] {
                WorkOrderStatus.Created,
                WorkOrderStatus.Scheduled
            };

            return await _dbSet
                .Where(w => pendingStatuses.Contains(w.Status))
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.ScheduledStartTime ?? w.CreatedAt)
                .ToListAsync();
        }

        // 재고별 조회
        public async Task<IEnumerable<WorkOrder>> GetByProductStockAsync(Guid productStockId)
        {
            return await _dbSet
                .Where(w => w.ProductStockId == productStockId)
                .Include(w => w.Recipe)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<WorkOrder?> GetActiveOrderByStockAsync(Guid productStockId)
        {
            var activeStatuses = new[] {
                WorkOrderStatus.Created,
                WorkOrderStatus.Scheduled,
                WorkOrderStatus.InProgress
            };

            return await _dbSet
                .Where(w => w.ProductStockId == productStockId && activeStatuses.Contains(w.Status))
                .Include(w => w.Recipe)
                .FirstOrDefaultAsync();
        }

        // 레시피별 조회
        public async Task<IEnumerable<WorkOrder>> GetByRecipeAsync(Guid recipeId)
        {
            return await _dbSet
                .Where(w => w.RecipeId == recipeId)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        // 우선순위별 조회
        public async Task<IEnumerable<WorkOrder>> GetHighPriorityOrdersAsync(int threshold = 70)
        {
            return await _dbSet
                .Where(w => w.Priority >= threshold &&
                           (w.Status == WorkOrderStatus.Created ||
                            w.Status == WorkOrderStatus.Scheduled))
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.CreatedAt)
                .ToListAsync();
        }

        // 날짜별 조회
        public async Task<IEnumerable<WorkOrder>> GetScheduledOrdersAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            return await _dbSet
                .Where(w => w.ScheduledStartTime >= startOfDay &&
                           w.ScheduledStartTime <= endOfDay)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderBy(w => w.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrder>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(w => (w.ActualStartTime >= startDate && w.ActualStartTime <= endDate) ||
                           (w.ScheduledStartTime >= startDate && w.ScheduledStartTime <= endDate))
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderBy(w => w.ScheduledStartTime ?? w.ActualStartTime)
                .ToListAsync();
        }

        // 현재 단계별 조회
        public async Task<IEnumerable<WorkOrder>> GetByCurrentStepAsync(int stepNumber)
        {
            return await _dbSet
                .Where(w => w.CurrentRecipeStep == stepNumber &&
                           w.Status == WorkOrderStatus.InProgress)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .ToListAsync();
        }

        // 진행률 조회
        public async Task<IEnumerable<WorkOrder>> GetOrdersByProgressAsync(decimal minProgress, decimal maxProgress)
        {
            return await _dbSet
                .Where(w => w.ProgressPercentage >= minProgress &&
                           w.ProgressPercentage <= maxProgress)
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                .OrderBy(w => w.ProgressPercentage)
                .ToListAsync();
        }

        // 다음 실행 가능한 작업 조회
        public async Task<WorkOrder?> GetNextExecutableOrderAsync()
        {
            var now = DateTime.UtcNow;

            return await _dbSet
                .Where(w => (w.Status == WorkOrderStatus.Created ||
                            w.Status == WorkOrderStatus.Scheduled) &&
                           (w.ScheduledStartTime == null || w.ScheduledStartTime <= now))
                .Include(w => w.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(w => w.Recipe)
                    .ThenInclude(r => r.Steps)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.ScheduledStartTime ?? w.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // 통계
        public async Task<Dictionary<WorkOrderStatus, int>> GetStatusStatisticsAsync()
        {
            return await _dbSet
                .GroupBy(w => w.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<int> GetCompletedOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(w => w.Status == WorkOrderStatus.Completed);

            if (startDate.HasValue)
            {
                query = query.Where(w => w.ActualEndTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(w => w.ActualEndTime <= endDate.Value);
            }

            return await query.CountAsync();
        }

        public async Task<decimal> GetAverageCompletionTimeAsync(DateTime? startDate = null)
        {
            var query = _dbSet
                .Where(w => w.Status == WorkOrderStatus.Completed &&
                           w.ActualStartTime.HasValue &&
                           w.ActualEndTime.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(w => w.ActualEndTime >= startDate.Value);
            }

            var completedOrders = await query.ToListAsync();

            if (!completedOrders.Any())
                return 0;

            var totalMinutes = completedOrders
                .Select(w => (w.ActualEndTime!.Value - w.ActualStartTime!.Value).TotalMinutes)
                .Average();

            return (decimal)totalMinutes;
        }

        // 번호 생성
        public async Task<string> GenerateOrderNumberAsync(string prefix = "WO")
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var pattern = $"{prefix}-{today}-%";

            var lastOrder = await _dbSet
                .Where(w => EF.Functions.Like(w.OrderNumber, pattern))
                .OrderByDescending(w => w.OrderNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Split('-').Last();
                if (int.TryParse(lastSequence, out var seq))
                {
                    sequence = seq + 1;
                }
            }

            return $"{prefix}-{today}-{sequence:D4}";
        }

        // 진행률 업데이트
        public async Task UpdateProgressAsync(Guid workOrderId, decimal progress)
        {
            var workOrder = await GetByIdAsync(workOrderId);
            if (workOrder != null)
            {
                workOrder.ProgressPercentage = Math.Min(100, Math.Max(0, progress));
                Update(workOrder);
                await SaveChangesAsync();
            }
        }

        // 상태 변경
        public async Task<bool> UpdateStatusAsync(Guid workOrderId, WorkOrderStatus newStatus)
        {
            var workOrder = await GetByIdAsync(workOrderId);
            if (workOrder == null)
                return false;

            workOrder.Status = newStatus;

            // 상태에 따른 추가 처리
            switch (newStatus)
            {
                case WorkOrderStatus.InProgress:
                    if (!workOrder.ActualStartTime.HasValue)
                        workOrder.ActualStartTime = DateTime.UtcNow;
                    break;

                case WorkOrderStatus.Completed:
                    workOrder.ActualEndTime = DateTime.UtcNow;
                    workOrder.ProgressPercentage = 100;

                    // 재고 상태 업데이트
                    var stock = await _context.ProductStocks.FindAsync(workOrder.ProductStockId);
                    if (stock != null)
                    {
                        stock.Status = StockStatus.Completed;
                        _context.ProductStocks.Update(stock);
                    }
                    break;

                case WorkOrderStatus.Cancelled:
                case WorkOrderStatus.Failed:
                    if (!workOrder.ActualEndTime.HasValue)
                        workOrder.ActualEndTime = DateTime.UtcNow;

                    // 재고 상태 원복
                    var failedStock = await _context.ProductStocks.FindAsync(workOrder.ProductStockId);
                    if (failedStock != null && failedStock.Status == StockStatus.InProcess)
                    {
                        failedStock.Status = StockStatus.Available;
                        _context.ProductStocks.Update(failedStock);
                    }
                    break;
            }

            Update(workOrder);
            await SaveChangesAsync();
            return true;
        }

        // 현재 단계 업데이트
        public async Task UpdateCurrentStepAsync(Guid workOrderId, int stepNumber)
        {
            var workOrder = await GetByIdAsync(workOrderId);
            if (workOrder != null)
            {
                workOrder.CurrentRecipeStep = stepNumber;

                // 진행률 계산
                var recipe = await _context.Recipes
                    .Include(r => r.Steps)
                    .FirstOrDefaultAsync(r => r.Id == workOrder.RecipeId);

                if (recipe != null && recipe.Steps.Any())
                {
                    var totalSteps = recipe.Steps.Count;
                    workOrder.ProgressPercentage = (decimal)(stepNumber - 1) / totalSteps * 100;
                }

                Update(workOrder);
                await SaveChangesAsync();
            }
        }

        // 작업 지시 생성
        public async Task<WorkOrder> CreateWorkOrderAsync(ProductStock productStock, Recipe recipe, int priority = 50)
        {
            var orderNumber = await GenerateOrderNumberAsync();

            var workOrder = new WorkOrder
            {
                OrderNumber = orderNumber,
                OrderName = $"{productStock.Product.ProductName} - {recipe.RecipeName}",
                ProductStockId = productStock.Id,
                ProductStock = productStock,
                RecipeId = recipe.Id,
                Recipe = recipe,
                Status = WorkOrderStatus.Created,
                Priority = priority,
                CurrentRecipeStep = 1,
                ProgressPercentage = 0,
                CreatedBy = "System"
            };

            // 예상 종료 시간 계산
            if (workOrder.ScheduledStartTime.HasValue)
            {
                workOrder.ScheduledEndTime = workOrder.ScheduledStartTime.Value.AddMinutes(recipe.TotalEstimatedMinutes);
            }

            await AddAsync(workOrder);

            // 재고 예약
            productStock.Status = StockStatus.Reserved;
            _context.ProductStocks.Update(productStock);

            // 공정 실행 레코드 생성
            foreach (var step in recipe.Steps.OrderBy(s => s.StepNumber))
            {
                var execution = new ProcessExecution
                {
                    WorkOrderId = workOrder.Id,
                    ProductStockId = productStock.Id,
                    RecipeStepId = step.Id,
                    Status = ExecutionStatus.Pending,
                    CreatedBy = "System"
                };

                _context.ProcessExecutions.Add(execution);
            }

            await SaveChangesAsync();
            return workOrder;
        }
    }
}