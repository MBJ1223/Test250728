using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// IntegrationLog 리포지토리 구현
    /// </summary>
    public class IntegrationLogRepository : BaseRepository<IntegrationLog>, IIntegrationLogRepository
    {
        public IntegrationLogRepository(MesDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 시스템별 통합 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetBySystemIntegrationIdAsync(
            Guid systemIntegrationId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 100)
        {
            var query = _dbSet
                .Where(log => log.SystemIntegrationId == systemIntegrationId);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.LogDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.LogDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.LogDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 액션별 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetByActionAsync(
            string action,
            IntegrationResult? result = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.Where(log => log.Action == action);

            if (result.HasValue)
            {
                query = query.Where(log => log.Result == result.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.LogDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.LogDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.LogDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 작업 지시별 통합 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetByWorkOrderIdAsync(Guid workOrderId)
        {
            return await _dbSet
                .Where(log => log.WorkOrderId == workOrderId)
                .OrderBy(log => log.LogDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 상관관계 ID로 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetByCorrelationIdAsync(string correlationId)
        {
            return await _dbSet
                .Where(log => log.CorrelationId == correlationId)
                .OrderBy(log => log.LogDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 실패한 통합 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetFailedIntegrationsAsync(
            DateTime startDate,
            DateTime endDate,
            Guid? systemIntegrationId = null)
        {
            var query = _dbSet
                .Where(log => log.Result == IntegrationResult.Failed &&
                             log.LogDate >= startDate &&
                             log.LogDate <= endDate);

            if (systemIntegrationId.HasValue)
            {
                query = query.Where(log => log.SystemIntegrationId == systemIntegrationId.Value);
            }

            return await query
                .OrderByDescending(log => log.LogDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 시스템별 통합 통계 조회
        /// </summary>
        public async Task<Dictionary<string, IntegrationStatistics>> GetIntegrationStatisticsAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var logs = await _dbSet
                .Where(log => log.LogDate >= startDate && log.LogDate <= endDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();

            var statistics = logs
                .GroupBy(log => log.SystemIntegration.SystemCode)
                .Select(group => new IntegrationStatistics
                {
                    SystemCode = group.Key,
                    TotalCalls = group.Count(),
                    SuccessCount = group.Count(log => log.Result == IntegrationResult.Success),
                    FailureCount = group.Count(log => log.Result == IntegrationResult.Failed),
                    TimeoutCount = group.Count(log => log.Result == IntegrationResult.Timeout),
                    AverageExecutionTimeMs = (long)group.Average(log => log.ExecutionTimeMs),
                    MaxExecutionTimeMs = group.Max(log => log.ExecutionTimeMs),
                    MinExecutionTimeMs = group.Min(log => log.ExecutionTimeMs)
                })
                .ToDictionary(stat => stat.SystemCode);

            return statistics;
        }

        /// <summary>
        /// 성능 문제가 있는 통합 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetSlowIntegrationsAsync(
            long thresholdMs,
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .Where(log => log.ExecutionTimeMs > thresholdMs &&
                             log.LogDate >= startDate &&
                             log.LogDate <= endDate)
                .OrderByDescending(log => log.ExecutionTimeMs)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// HTTP 메소드별 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetByHttpMethodAsync(
            string httpMethod,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.Where(log => log.HttpMethod == httpMethod);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.LogDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.LogDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.LogDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 최근 통합 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetRecentLogsAsync(
            int count = 100,
            Guid? systemIntegrationId = null)
        {
            var query = _dbSet.AsQueryable();

            if (systemIntegrationId.HasValue)
            {
                query = query.Where(log => log.SystemIntegrationId == systemIntegrationId.Value);
            }

            return await query
                .OrderByDescending(log => log.LogDate)
                .Take(count)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 엔드포인트별 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetByEndpointAsync(
            string endpoint,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.Where(log => log.Endpoint == endpoint);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.LogDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.LogDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.LogDate)
                .Include(log => log.SystemIntegration)
                .ToListAsync();
        }

        /// <summary>
        /// 오래된 로그 정리
        /// </summary>
        public async Task<int> CleanupOldLogsAsync(DateTime cutoffDate)
        {
            var logsToDelete = await _dbSet
                .Where(log => log.LogDate < cutoffDate &&
                             log.Result == IntegrationResult.Success)
                .ToListAsync();

            if (logsToDelete.Any())
            {
                _dbSet.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
            }

            return logsToDelete.Count;
        }

        /// <summary>
        /// 시스템 헬스 체크 로그 조회
        /// </summary>
        public async Task<List<IntegrationLog>> GetHealthCheckLogsAsync(
            Guid systemIntegrationId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _dbSet
                .Where(log => log.SystemIntegrationId == systemIntegrationId &&
                             log.Action == "HealthCheck" &&
                             log.LogDate >= startDate &&
                             log.LogDate <= endDate)
                .OrderByDescending(log => log.LogDate)
                .ToListAsync();
        }
    }
}