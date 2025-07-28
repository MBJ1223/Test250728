using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// WorkOrderExecution Repository 인터페이스
    /// </summary>
    public interface IWorkOrderExecutionRepository : IBaseRepository<WorkOrderExecution>
    {
        // 작업별 실행 조회
        Task<IEnumerable<WorkOrderExecution>> GetByWorkOrderAsync(Guid workOrderId);
        Task<IEnumerable<WorkOrderExecution>> GetByWorkOrderWithDetailsAsync(Guid workOrderId);

        // 현재 실행 중인 작업 조회
        Task<IEnumerable<WorkOrderExecution>> GetActiveExecutionsAsync();
        Task<WorkOrderExecution?> GetCurrentExecutionAsync(Guid workOrderId);

        // 상태별 조회
        Task<IEnumerable<WorkOrderExecution>> GetByStatusAsync(ExecutionStatus status);
        Task<IEnumerable<WorkOrderExecution>> GetPendingExecutionsAsync(Guid workOrderId);

        // 단계별 조회
        Task<WorkOrderExecution?> GetByStepAsync(Guid workOrderId, Guid scenarioStepId);
        Task<IEnumerable<WorkOrderExecution>> GetByStepNumberAsync(Guid workOrderId, int stepNumber);

        // 리소스별 조회
        Task<IEnumerable<WorkOrderExecution>> GetByResourceAsync(string resourceCode);
        Task<IEnumerable<WorkOrderExecution>> GetActiveExecutionsByResourceAsync(string resourceCode);

        // 실패한 실행 조회
        Task<IEnumerable<WorkOrderExecution>> GetFailedExecutionsAsync(DateTime? since = null);
        Task<IEnumerable<WorkOrderExecution>> GetRetryableExecutionsAsync();

        // 다음 실행 가능한 단계 조회
        Task<WorkOrderExecution?> GetNextExecutableStepAsync(Guid workOrderId);

        // 실행 시작/종료
        Task<bool> StartExecutionAsync(Guid executionId, string? assignedResource = null);
        Task<bool> CompleteExecutionAsync(Guid executionId, JsonDocument? resultData = null);
        Task<bool> FailExecutionAsync(Guid executionId, string errorMessage, string? errorDetail = null);

        // 재시도
        Task<bool> RetryExecutionAsync(Guid executionId);
        Task<int> GetRetryCountAsync(Guid executionId);

        // 실행 데이터 업데이트
        Task UpdateExecutionDataAsync(Guid executionId, JsonDocument executionData);
        Task AppendExecutionDataAsync(Guid executionId, string key, object value);

        // 통계
        Task<Dictionary<ExecutionStatus, int>> GetStatusStatisticsByWorkOrderAsync(Guid workOrderId);
        Task<TimeSpan> GetAverageExecutionTimeAsync(Guid scenarioStepId);
        Task<double> GetSuccessRateAsync(Guid scenarioStepId, DateTime? since = null);

        // 로그 추가
        Task AddExecutionLogAsync(Guid executionId, LogLevel level, string message, JsonDocument? additionalData = null);
    }
}