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
    /// WorkOrderExecution Repository 구현체
    /// </summary>
    public class WorkOrderExecutionRepository : BaseRepository<WorkOrderExecution>, IWorkOrderExecutionRepository
    {
        public WorkOrderExecutionRepository(MesDbContext context) : base(context)
        {
        }

        // 작업별 실행 조회
        public async Task<IEnumerable<WorkOrderExecution>> GetByWorkOrderAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId)
                .Include(e => e.ScenarioStep)
                .OrderBy(e => e.ScenarioStep.StepNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrderExecution>> GetByWorkOrderWithDetailsAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId)
                .Include(e => e.ScenarioStep)
                .Include(e => e.Logs)
                .OrderBy(e => e.ScenarioStep.StepNumber)
                .ToListAsync();
        }

        // 현재 실행 중인 작업 조회
        public async Task<IEnumerable<WorkOrderExecution>> GetActiveExecutionsAsync()
        {
            return await _dbSet
                .Where(e => e.Status == ExecutionStatus.InProgress)
                .Include(e => e.WorkOrder)
                .Include(e => e.ScenarioStep)
                .ToListAsync();
        }

        public async Task<WorkOrderExecution?> GetCurrentExecutionAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.Status == ExecutionStatus.InProgress)
                .Include(e => e.ScenarioStep)
                .FirstOrDefaultAsync();
        }

        // 상태별 조회
        public async Task<IEnumerable<WorkOrderExecution>> GetByStatusAsync(ExecutionStatus status)
        {
            return await _dbSet
                .Where(e => e.Status == status)
                .Include(e => e.WorkOrder)
                .Include(e => e.ScenarioStep)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrderExecution>> GetPendingExecutionsAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.Status == ExecutionStatus.Pending)
                .Include(e => e.ScenarioStep)
                .OrderBy(e => e.ScenarioStep.StepNumber)
                .ToListAsync();
        }

        // 단계별 조회
        public async Task<WorkOrderExecution?> GetByStepAsync(Guid workOrderId, Guid scenarioStepId)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.ScenarioStepId == scenarioStepId)
                .Include(e => e.ScenarioStep)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WorkOrderExecution>> GetByStepNumberAsync(Guid workOrderId, int stepNumber)
        {
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.ScenarioStep.StepNumber == stepNumber)
                .Include(e => e.ScenarioStep)
                .ToListAsync();
        }

        // 리소스별 조회
        public async Task<IEnumerable<WorkOrderExecution>> GetByResourceAsync(string resourceCode)
        {
            return await _dbSet
                .Where(e => e.AssignedResource == resourceCode)
                .Include(e => e.WorkOrder)
                .Include(e => e.ScenarioStep)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrderExecution>> GetActiveExecutionsByResourceAsync(string resourceCode)
        {
            return await _dbSet
                .Where(e => e.AssignedResource == resourceCode && e.Status == ExecutionStatus.InProgress)
                .Include(e => e.WorkOrder)
                .Include(e => e.ScenarioStep)
                .ToListAsync();
        }

        // 실패한 실행 조회
        public async Task<IEnumerable<WorkOrderExecution>> GetFailedExecutionsAsync(DateTime? since = null)
        {
            var query = _dbSet.Where(e => e.Status == ExecutionStatus.Failed);

            if (since.HasValue)
            {
                query = query.Where(e => e.EndTime >= since.Value);
            }

            return await query
                .Include(e => e.WorkOrder)
                .Include(e => e.ScenarioStep)
                .OrderByDescending(e => e.EndTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkOrderExecution>> GetRetryableExecutionsAsync()
        {
            return await _dbSet
                .Where(e => e.Status == ExecutionStatus.Failed &&
                           e.RetryCount < e.ScenarioStep.MaxRetryCount)
                .Include(e => e.WorkOrder)
                .Include(e => e.ScenarioStep)
                .ToListAsync();
        }

        // 다음 실행 가능한 단계 조회
        public async Task<WorkOrderExecution?> GetNextExecutableStepAsync(Guid workOrderId)
        {
            // 현재 진행 중인 실행이 있는지 확인
            var currentExecution = await GetCurrentExecutionAsync(workOrderId);
            if (currentExecution != null)
                return null;

            // 대기 중인 실행 중 가장 낮은 단계 번호 찾기
            return await _dbSet
                .Where(e => e.WorkOrderId == workOrderId && e.Status == ExecutionStatus.Pending)
                .Include(e => e.ScenarioStep)
                .OrderBy(e => e.ScenarioStep.StepNumber)
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

            Update(execution);
            await SaveChangesAsync();

            // 로그 추가
            await AddExecutionLogAsync(executionId, LogLevel.Info, "실행 시작",
                JsonSerializer.SerializeToDocument(new { Resource = assignedResource }));

            return true;
        }

        public async Task<bool> CompleteExecutionAsync(Guid executionId, JsonDocument? resultData = null)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null || execution.Status != ExecutionStatus.InProgress)
                return false;

            execution.Status = ExecutionStatus.Completed;
            execution.EndTime = DateTime.UtcNow;

            if (resultData != null)
                execution.ResultData = resultData;

            Update(execution);
            await SaveChangesAsync();

            // 로그 추가
            await AddExecutionLogAsync(executionId, LogLevel.Info, "실행 완료");

            return true;
        }

        public async Task<bool> FailExecutionAsync(Guid executionId, string errorMessage, string? errorDetail = null)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null)
                return false;

            execution.Status = ExecutionStatus.Failed;
            execution.EndTime = DateTime.UtcNow;
            execution.ErrorMessage = errorMessage;
            execution.ErrorDetail = errorDetail;

            Update(execution);
            await SaveChangesAsync();

            // 로그 추가
            await AddExecutionLogAsync(executionId, LogLevel.Error, $"실행 실패: {errorMessage}",
                JsonSerializer.SerializeToDocument(new { ErrorDetail = errorDetail }));

            return true;
        }

        // 재시도
        public async Task<bool> RetryExecutionAsync(Guid executionId)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null || execution.Status != ExecutionStatus.Failed)
                return false;

            var scenarioStep = await _context.WorkScenarioSteps
                .FirstOrDefaultAsync(s => s.Id == execution.ScenarioStepId);

            if (scenarioStep == null || execution.RetryCount >= scenarioStep.MaxRetryCount)
                return false;

            execution.Status = ExecutionStatus.Pending;
            execution.RetryCount++;
            execution.ErrorMessage = null;
            execution.ErrorDetail = null;
            execution.StartTime = null;
            execution.EndTime = null;

            Update(execution);
            await SaveChangesAsync();

            // 로그 추가
            await AddExecutionLogAsync(executionId, LogLevel.Warning, $"재시도 {execution.RetryCount}회");

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

        public async Task<TimeSpan> GetAverageExecutionTimeAsync(Guid scenarioStepId)
        {
            var completedExecutions = await _dbSet
                .Where(e => e.ScenarioStepId == scenarioStepId &&
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

        public async Task<double> GetSuccessRateAsync(Guid scenarioStepId, DateTime? since = null)
        {
            var query = _dbSet.Where(e => e.ScenarioStepId == scenarioStepId &&
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

        // 로그 추가
        public async Task AddExecutionLogAsync(Guid executionId, LogLevel level, string message, JsonDocument? additionalData = null)
        {
            var execution = await GetByIdAsync(executionId);
            if (execution == null)
                return;

            var log = new ExecutionLog
            {
                WorkOrderId = execution.WorkOrderId,
                WorkOrderExecutionId = executionId,
                LogLevel = level,
                Category = "Execution",
                EventType = "ExecutionLog",
                Message = message,
                AdditionalData = additionalData,
                SourceSystem = "MES"
            };

            _context.ExecutionLogs.Add(log);
            await SaveChangesAsync();
        }
    }
}