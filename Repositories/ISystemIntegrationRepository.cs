using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// SystemIntegration Repository 인터페이스
    /// </summary>
    public interface ISystemIntegrationRepository : IBaseRepository<SystemIntegration>
    {
        // 시스템 코드로 조회
        Task<SystemIntegration?> GetBySystemCodeAsync(string systemCode);

        // 활성 시스템 조회
        Task<IEnumerable<SystemIntegration>> GetActiveSystemsAsync();

        // 통합 타입별 조회
        Task<IEnumerable<SystemIntegration>> GetByIntegrationTypeAsync(IntegrationType type);

        // 헬스 상태별 조회
        Task<IEnumerable<SystemIntegration>> GetUnhealthySystemsAsync();

        // 헬스체크 업데이트
        Task UpdateHealthCheckAsync(Guid systemId, bool isHealthy);
        Task UpdateLastCommunicationAsync(Guid systemId);

        // 상태 변경
        Task<bool> SetStatusAsync(Guid systemId, IntegrationStatus status);

        // 시스템별 로그 조회
        Task<IEnumerable<IntegrationLog>> GetRecentLogsAsync(Guid systemId, int count = 100);
        Task<IEnumerable<IntegrationLog>> GetErrorLogsAsync(Guid systemId, DateTime since);

        // 통계
        Task<Dictionary<string, int>> GetCommunicationStatisticsAsync(DateTime since);
        Task<double> GetSuccessRateAsync(Guid systemId, DateTime since);
    }
}