using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// ProcessExecution Repository 인터페이스
    /// </summary>
    public interface IProcessExecutionRepository : IBaseRepository<ProcessExecution>
    {
        // 작업별 실행 조회
        Task<IEnumerable<ProcessExecution>> GetByWorkOrderAsync(Guid workOrderId);
        Task<IEnumerable<ProcessExecution>> GetByWorkOrderWithDetailsAsync(Guid workOrderId);

        // 재고별 실행 조회
        Task<IEnumerable<ProcessExecution>> GetByProductStockAsync(Guid productStockId);
        Task<ProcessExecution?> GetCurrentExecutionByStockAsync(Guid productStockId);

        // 현재 실행 중인 공정 조회
        Task<IEnumerable<ProcessExecution>> GetActiveExecutionsAsync();
        Task<ProcessExecution?> GetCurrentExecutionAsync(Guid workOrderId);

        // 상태별 조회
        Task<IEnumerable<ProcessExecution>> GetByStatusAsync(ExecutionStatus status);
        Task<IEnumerable<ProcessExecution>> GetPendingExecutionsAsync(Guid workOrderId);

        // 단계별 조회
        Task<ProcessExecution?> GetByRecipeStepAsync(Guid workOrderId, Guid recipeStepId);
        Task<IEnumerable<ProcessExecution>> GetByStepNumberAsync(Guid workOrderId, int stepNumber);

        // 리소스별 조회
        Task<IEnumerable<ProcessExecution>> GetByResourceAsync(string resourceCode);
        Task<IEnumerable<ProcessExecution>> GetActiveExecutionsByResourceAsync(string resourceCode);

        // 실패한 실행 조회
        Task<IEnumerable<ProcessExecution>> GetFailedExecutionsAsync(DateTime? since = null);
        Task<IEnumerable<ProcessExecution>> GetRetryableExecutionsAsync();

        // 다음 실행 가능한 단계 조회
        Task<ProcessExecution?> GetNextExecutableStepAsync(Guid workOrderId);

        // 실행 시작/종료
        Task<bool> StartExecutionAsync(Guid executionId, string? assignedResource = null);
        Task<bool> CompleteExecutionAsync(Guid executionId, int? endLocationId = null, JsonDocument? resultData = null);
        Task<bool> FailExecutionAsync(Guid executionId, string errorMessage);

        // 재시도
        Task<bool> RetryExecutionAsync(Guid executionId);
        Task<int> GetRetryCountAsync(Guid executionId);

        // 실행 데이터 업데이트
        Task UpdateExecutionDataAsync(Guid executionId, JsonDocument executionData);
        Task AppendExecutionDataAsync(Guid executionId, string key, object value);

        // 통계
        Task<Dictionary<ExecutionStatus, int>> GetStatusStatisticsByWorkOrderAsync(Guid workOrderId);
        Task<TimeSpan> GetAverageExecutionTimeAsync(Guid recipeStepId);
        Task<double> GetSuccessRateAsync(Guid recipeStepId, DateTime? since = null);

        // 위치별 공정 조회
        Task<IEnumerable<ProcessExecution>> GetByLocationAsync(int locationId);
        Task<IEnumerable<ProcessExecution>> GetCompletedAtLocationAsync(int locationId, DateTime? since = null);
    }
}