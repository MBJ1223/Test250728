using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// ExecutionLog Repository 인터페이스
    /// </summary>
    public interface IExecutionLogRepository : IBaseRepository<ExecutionLog>
    {
        // 작업별 로그 조회
        Task<IEnumerable<ExecutionLog>> GetByWorkOrderAsync(Guid workOrderId, LogLevel? minLevel = null);
        Task<IEnumerable<ExecutionLog>> GetByExecutionAsync(Guid executionId, LogLevel? minLevel = null);

        // 시간대별 조회
        Task<IEnumerable<ExecutionLog>> GetLogsInDateRangeAsync(DateTime startDate, DateTime endDate, LogLevel? minLevel = null);
        Task<IEnumerable<ExecutionLog>> GetRecentLogsAsync(int count = 100, LogLevel? minLevel = null);

        // 레벨별 조회
        Task<IEnumerable<ExecutionLog>> GetByLogLevelAsync(LogLevel level, DateTime? since = null);
        Task<IEnumerable<ExecutionLog>> GetErrorLogsAsync(DateTime? since = null);
        Task<IEnumerable<ExecutionLog>> GetCriticalLogsAsync(DateTime? since = null);

        // 카테고리별 조회
        Task<IEnumerable<ExecutionLog>> GetByCategoryAsync(string category, DateTime? since = null);

        // 이벤트 타입별 조회
        Task<IEnumerable<ExecutionLog>> GetByEventTypeAsync(string eventType, DateTime? since = null);

        // 시스템별 조회
        Task<IEnumerable<ExecutionLog>> GetBySourceSystemAsync(string sourceSystem, DateTime? since = null);
        Task<IEnumerable<ExecutionLog>> GetBySystemsAsync(string sourceSystem, string targetSystem);

        // 상관관계별 조회
        Task<IEnumerable<ExecutionLog>> GetByCorrelationIdAsync(string correlationId);

        // 로그 추가 헬퍼
        Task AddInfoLogAsync(string message, string eventType, Guid? workOrderId = null, JsonDocument? data = null);
        Task AddWarningLogAsync(string message, string eventType, Guid? workOrderId = null, JsonDocument? data = null);
        Task AddErrorLogAsync(string message, string eventType, Exception? exception = null, Guid? workOrderId = null);

        // 통계
        Task<Dictionary<LogLevel, int>> GetLogLevelStatisticsAsync(DateTime since);
        Task<Dictionary<string, int>> GetEventTypeStatisticsAsync(DateTime since);
        Task<long> GetAverageExecutionTimeAsync(string eventType, DateTime since);

        // 정리
        Task<int> CleanupOldLogsAsync(DateTime olderThan, LogLevel? maxLevel = null);
    }
}