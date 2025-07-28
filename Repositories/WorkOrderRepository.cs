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
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .FirstOrDefaultAsync(w => w.OrderNumber == orderNumber);
        }

        // 상세 정보 포함 조회
        public async Task<WorkOrder?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                    .ThenInclude(s => s.Steps)
                .Include(w => w.Executions)
                    .ThenInclude(e => e.ScenarioStep)
                .Include(w => w.Logs)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<WorkOrder?> GetWithExecutionsAsync(Guid id)
        {
            return await _dbSet
                .Include(w => w.Executions)
                    .ThenInclude(e => e.ScenarioStep)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        // 상태별 조회
        public async Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status)
        {
            return await _dbSet
                .Where(w => w.Status == status)
                .Include(w => w.Product)
                .Include(w => w.Scenario)
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
                .Include(w => w.Product)
                .Include(w => w.Scenario)
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
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.ScheduledStartTime ?? w.CreatedAt)
                .ToListAsync();
        }

        // 우선순위별 조회
        public async Task<IEnumerable<WorkOrder>> GetHighPriorityOrdersAsync(int threshold = 70)
        {
            return await _dbSet
                .Where(w => w.Priority >= threshold &&
                           (w.Status == WorkOrderStatus.Created ||
                            w.Status == WorkOrderStatus.Scheduled))
                .Include(w => w.Product)
                .Include(w => w.Scenario)
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
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .OrderBy(w => w.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrder>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(w => (w.ActualStartTime >= startDate && w.ActualStartTime <= endDate) ||
                           (w.ScheduledStartTime >= startDate && w.ScheduledStartTime <= endDate))
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .OrderBy(w => w.ScheduledStartTime ?? w.ActualStartTime)
                .ToListAsync();
        }

        // 제품별 조회
        public async Task<IEnumerable<WorkOrder>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(w => w.ProductId == productId)
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        // 시나리오별 조회
        public async Task<IEnumerable<WorkOrder>> GetByScenarioAsync(Guid scenarioId)
        {
            return await _dbSet
                .Where(w => w.ScenarioId == scenarioId)
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        // 현재 단계별 조회
        public async Task<IEnumerable<WorkOrder>> GetByCurrentStepAsync(int stepNumber)
        {
            return await _dbSet
                .Where(w => w.CurrentStepNumber == stepNumber &&
                           w.Status == WorkOrderStatus.InProgress)
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                .ToListAsync();
        }

        // 진행률 조회
        public async Task<IEnumerable<WorkOrder>> GetOrdersByProgressAsync(decimal minProgress, decimal maxProgress)
        {
            return await _dbSet
                .Where(w => w.ProgressPercentage >= minProgress &&
                           w.ProgressPercentage <= maxProgress)
                .Include(w => w.Product)
                .Include(w => w.Scenario)
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
                .Include(w => w.Product)
                .Include(w => w.Scenario)
                    .ThenInclude(s => s.Steps)
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
                    break;

                case WorkOrderStatus.Cancelled:
                case WorkOrderStatus.Failed:
                    if (!workOrder.ActualEndTime.HasValue)
                        workOrder.ActualEndTime = DateTime.UtcNow;
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
                workOrder.CurrentStepNumber = stepNumber;
                Update(workOrder);
                await SaveChangesAsync();
            }
        }
    }
}