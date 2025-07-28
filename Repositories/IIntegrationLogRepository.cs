using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// IntegrationLog 리포지토리 인터페이스
    /// </summary>
    public interface IIntegrationLogRepository : IBaseRepository<IntegrationLog>
    {
        /// <summary>
        /// 시스템별 통합 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetBySystemIntegrationIdAsync(
            Guid systemIntegrationId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 100);

        /// <summary>
        /// 액션별 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetByActionAsync(
            string action,
            IntegrationResult? result = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 작업 지시별 통합 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetByWorkOrderIdAsync(Guid workOrderId);

        /// <summary>
        /// 상관관계 ID로 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetByCorrelationIdAsync(string correlationId);

        /// <summary>
        /// 실패한 통합 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetFailedIntegrationsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? systemIntegrationId = null);

        /// <summary>
        /// 시스템별 통합 통계 조회
        /// </summary>
        Task<Dictionary<string, IntegrationStatistics>> GetIntegrationStatisticsAsync(
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// 성능 문제가 있는 통합 조회 (실행 시간이 임계값을 초과한 경우)
        /// </summary>
        Task<List<IntegrationLog>> GetSlowIntegrationsAsync(
            long thresholdMs,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// HTTP 메소드별 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetByHttpMethodAsync(
            string httpMethod,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 최근 통합 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetRecentLogsAsync(
            int count = 100,
            Guid? systemIntegrationId = null);

        /// <summary>
        /// 엔드포인트별 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetByEndpointAsync(
            string endpoint,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 오래된 로그 정리
        /// </summary>
        Task<int> CleanupOldLogsAsync(DateTime cutoffDate);

        /// <summary>
        /// 시스템 헬스 체크 로그 조회
        /// </summary>
        Task<List<IntegrationLog>> GetHealthCheckLogsAsync(
            Guid systemIntegrationId,
            DateTime startDate,
            DateTime endDate);
    }

    /// <summary>
    /// 통합 통계 DTO
    /// </summary>
    public class IntegrationStatistics
    {
        public string SystemCode { get; set; } = string.Empty;
        public int TotalCalls { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int TimeoutCount { get; set; }
        public double SuccessRate => TotalCalls > 0 ? (double)SuccessCount / TotalCalls * 100 : 0;
        public long AverageExecutionTimeMs { get; set; }
        public long MaxExecutionTimeMs { get; set; }
        public long MinExecutionTimeMs { get; set; }
    }
}