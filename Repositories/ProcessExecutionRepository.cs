using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// ProcessExecution Repository 구현체
    /// </summary>
    public class ProcessExecutionRepository : BaseRepository<ProcessExecution>, IProcessExecutionRepository
    {
        public ProcessExecutionRepository(MesDbContext context) : base(context)
        {
        }

        // 작업별 실행 조회
        public async Task<IEnumerable<ProcessExecution>> GetByWorkOrderAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId)
                .Include(e => e.RecipeStep)
                .OrderBy(e => e.RecipeStep.StepNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessExecution>> GetByWorkOrderWithDetailsAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId)
                .Include(e => e.RecipeStep)
                .Include(e => e.ProductStock)
                    .ThenInclude(s => s.Product)
                .Include(e => e.StartLocation)
                .Include(e => e.EndLocation)
                .OrderBy(e => e.RecipeStep.StepNumber)
                .ToListAsync();
        }

        // 재고별 실행 조회
        public async Task<IEnumerable<ProcessExecution>> GetByProductStockAsync(Guid productStockId)
        {
            return await _dbSet
                .Where(e => e.ProductStockId == productStockId)
                .Include(e => e.RecipeStep)
                .Include(e => e.WorkOrder)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProcessExecution?> GetCurrentExecutionByStockAsync(Guid productStockId)
        {
            return await _dbSet
                .Where(e => e.ProductStockId == productStockId && e.Status == ExecutionStatus.InProgress)
                .Include(e => e.RecipeStep)
                .Include(e => e.WorkOrder)
                .FirstOrDefaultAsync();
        }

        // 현재 실행 중인 공정 조회
        public async Task<IEnumerable<ProcessExecution>> GetActiveExecutionsAsync()
        {
            return await _dbSet
                .Where(e => e.Status == ExecutionStatus.InProgress)
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .Include(e => e.ProductStock)
                .ToListAsync();
        }

        public async Task<ProcessExecution?> GetCurrentExecutionAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.Status == ExecutionStatus.InProgress)
                .Include(e => e.RecipeStep)
                .FirstOrDefaultAsync();
        }

        // 상태별 조회
        public async Task<IEnumerable<ProcessExecution>> GetByStatusAsync(ExecutionStatus status)
        {
            return await _dbSet
                .Where(e => e.Status == status)
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .Include(e => e.ProductStock)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessExecution>> GetPendingExecutionsAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.Status == ExecutionStatus.Pending)
                .Include(e => e.RecipeStep)
                .OrderBy(e => e.RecipeStep.StepNumber)
                .ToListAsync();
        }

        // 단계별 조회
        public async Task<ProcessExecution?> GetByRecipeStepAsync(Guid workOrderId, Guid recipeStepId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.RecipeStepId == recipeStepId)
                .Include(e => e.RecipeStep)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProcessExecution>> GetByStepNumberAsync(Guid workOrderId, int stepNumber)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.RecipeStep.StepNumber == stepNumber)
                .Include(e => e.RecipeStep)
                .ToListAsync();
        }

        // 리소스별 조회
        public async Task<IEnumerable<ProcessExecution>> GetByResourceAsync(string resourceCode)
        {
            return await _dbSet
                .Where(e => e.AssignedResource == resourceCode)
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessExecution>> GetActiveExecutionsByResourceAsync(string resourceCode)
        {
            return await _dbSet
                .Where(e => e.AssignedResource == resourceCode && e.Status == ExecutionStatus.InProgress)
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .ToListAsync();
        }

        // 실패한 실행 조회
        public async Task<IEnumerable<ProcessExecution>> GetFailedExecutionsAsync(DateTime? since = null)
        {
            var query = _dbSet.Where(e => e.Status == ExecutionStatus.Failed);

            if (since.HasValue)
            {
                query = query.Where(e => e.EndTime >= since.Value);
            }

            return await query
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .OrderByDescending(e => e.EndTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessExecution>> GetRetryableExecutionsAsync()
        {
            return await _dbSet
                .Where(e => e.Status == ExecutionStatus.Failed && e.RetryCount < 3)
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .ToListAsync();
        }

        // 다음 실행 가능한 단계 조회
        public async Task<ProcessExecution?> GetNextExecutableStepAsync(Guid workOrderId)
        {
            var currentExecution = await GetCurrentExecutionAsync(workOrderId);
            if (currentExecution != null)
                return null;

            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.Status == ExecutionStatus.Pending)
                .Include(e => e.RecipeStep)
                .OrderBy(e => e.RecipeStep.StepNumber)
                .FirstOrDefaultAsync();
        }

        // 실행 시작/종료
        public async Task<bool> StartExecutionAsync(Guid executionId, string? assignedResource = null)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null || execution.Status != ExecutionStatus.Pending)
                return false;

            execution.Status = ExecutionStatus.InProgress;
            execution.StartTime = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(assignedResource))
                execution.AssignedResource = assignedResource;

            // 재고 상태 업데이트
            var stock = await _context.ProductStocks.FindAsync(execution.ProductStockId);
            if (stock != null)
            {
                stock.Status = StockStatus.InProcess;
                stock.CurrentRecipeStep = execution.RecipeStep?.StepNumber;
                _context.ProductStocks.Update(stock);
            }

            Update(execution);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompleteExecutionAsync(Guid executionId, int? endLocationId = null, JsonDocument? resultData = null)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null || execution.Status != ExecutionStatus.InProgress)
                return false;

            execution.Status = ExecutionStatus.Completed;
            execution.EndTime = DateTime.UtcNow;

            if (endLocationId.HasValue)
                execution.EndLocationId = endLocationId.Value;

            if (resultData != null)
                execution.ResultData = resultData;

            Update(execution);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> FailExecutionAsync(Guid executionId, string errorMessage)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null)
                return false;

            execution.Status = ExecutionStatus.Failed;
            execution.EndTime = DateTime.UtcNow;
            execution.ErrorMessage = errorMessage;

            Update(execution);
            await SaveChangesAsync();

            return true;
        }

        // 재시도
        public async Task<bool> RetryExecutionAsync(Guid executionId)
        {
            var execution = await GetByIdWithIncludesAsync(executionId, e => e.RecipeStep);
            if (execution == null || execution.Status != ExecutionStatus.Failed)
                return false;

            execution.Status = ExecutionStatus.Pending;
            execution.RetryCount++;
            execution.ErrorMessage = null;
            execution.StartTime = null;
            execution.EndTime = null;

            Update(execution);
            await SaveChangesAsync();

            return true;
        }

        public async Task<int> GetRetryCountAsync(Guid executionId)
        {
            var execution = await GetByIdAsync(executionId);
            return execution?.RetryCount ?? 0;
        }

        // 실행 데이터 업데이트
        public async Task UpdateExecutionDataAsync(Guid executionId, JsonDocument executionData)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null)
                return;

            execution.ExecutionData = executionData;
            Update(execution);
            await SaveChangesAsync();
        }

        public async Task AppendExecutionDataAsync(Guid executionId, string key, object value)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null)
                return;

            Dictionary<string, object> data;

            if (execution.ExecutionData != null)
            {
                var json = execution.ExecutionData.RootElement.GetRawText();
                data = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            else
            {
                data = new Dictionary<string, object>();
            }

            data[key] = value;
            execution.ExecutionData = JsonSerializer.SerializeToDocument(data);

            Update(execution);
            await SaveChangesAsync();
        }

        // 통계
        public async Task<Dictionary<ExecutionStatus, int>> GetStatusStatisticsByWorkOrderAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId)
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<TimeSpan> GetAverageExecutionTimeAsync(Guid recipeStepId)
        {
            var completedExecutions = await _dbSet
                .Where(e => e.RecipeStepId == recipeStepId &&
                           e.Status == ExecutionStatus.Completed &&
                           e.StartTime.HasValue &&
                           e.EndTime.HasValue)
                .ToListAsync();

            if (!completedExecutions.Any())
                return TimeSpan.Zero;

            var totalTime = completedExecutions
                .Select(e => e.EndTime!.Value - e.StartTime!.Value)
                .Aggregate(TimeSpan.Zero, (acc, time) => acc + time);

            return TimeSpan.FromMilliseconds(totalTime.TotalMilliseconds / completedExecutions.Count);
        }

        public async Task<double> GetSuccessRateAsync(Guid recipeStepId, DateTime? since = null)
        {
            var query = _dbSet.Where(e => e.RecipeStepId == recipeStepId &&
                                          (e.Status == ExecutionStatus.Completed || e.Status == ExecutionStatus.Failed));

            if (since.HasValue)
            {
                query = query.Where(e => e.EndTime >= since.Value);
            }

            var executions = await query.ToListAsync();

            if (!executions.Any())
                return 0;

            var successCount = executions.Count(e => e.Status == ExecutionStatus.Completed);
            return (double)successCount / executions.Count * 100;
        }

        // 위치별 공정 조회
        public async Task<IEnumerable<ProcessExecution>> GetByLocationAsync(int locationId)
        {
            return await _dbSet
                .Where(e => e.StartLocationId == locationId || e.EndLocationId == locationId)
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessExecution>> GetCompletedAtLocationAsync(int locationId, DateTime? since = null)
        {
            var query = _dbSet
                .Where(e => e.EndLocationId == locationId && e.Status == ExecutionStatus.Completed);

            if (since.HasValue)
            {
                query = query.Where(e => e.EndTime >= since.Value);
            }

            return await query
                .Include(e => e.WorkOrder)
                .Include(e => e.RecipeStep)
                .OrderByDescending(e => e.EndTime)
                .ToListAsync();
        }
    }
}