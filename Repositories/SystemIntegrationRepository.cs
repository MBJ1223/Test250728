using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// SystemIntegration Repository 구현체
    /// </summary>
    public class SystemIntegrationRepository : BaseRepository<SystemIntegration>, ISystemIntegrationRepository
    {
        public SystemIntegrationRepository(MesDbContext context) : base(context)
        {
        }

        // 시스템 코드로 조회
        public async Task<SystemIntegration?> GetBySystemCodeAsync(string systemCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.SystemCode == systemCode);
        }

        // 활성 시스템 조회
        public async Task<IEnumerable<SystemIntegration>> GetActiveSystemsAsync()
        {
            return await _dbSet
                .Where(s => s.Status == IntegrationStatus.Active)
                .OrderBy(s => s.SystemCode)
                .ToListAsync();
        }

        // 통합 타입별 조회
        public async Task<IEnumerable<SystemIntegration>> GetByIntegrationTypeAsync(IntegrationType type)
        {
            return await _dbSet
                .Where(s => s.IntegrationType == type)
                .OrderBy(s => s.SystemCode)
                .ToListAsync();
        }

        // 헬스 상태별 조회
        public async Task<IEnumerable<SystemIntegration>> GetUnhealthySystemsAsync()
        {
            return await _dbSet
                .Where(s => !s.IsHealthy && s.Status == IntegrationStatus.Active)
                .OrderBy(s => s.SystemCode)
                .ToListAsync();
        }

        // 헬스체크 업데이트
        public async Task UpdateHealthCheckAsync(Guid systemId, bool isHealthy)
        {
            var system = await GetByIdAsync(systemId);
            if (system == null)
                return;

            system.IsHealthy = isHealthy;
            system.LastHealthCheck = DateTime.UtcNow;

            // 헬스체크 실패 시 자동으로 Error 상태로 변경
            if (!isHealthy && system.Status == IntegrationStatus.Active)
            {
                system.Status = IntegrationStatus.Error;
            }
            // 헬스체크 성공 시 Error 상태면 Active로 복구
            else if (isHealthy && system.Status == IntegrationStatus.Error)
            {
                system.Status = IntegrationStatus.Active;
            }

            Update(system);
            await SaveChangesAsync();

            // 로그 추가
            var log = new IntegrationLog
            {
                SystemIntegrationId = systemId,
                Action = "HealthCheck",
                Result = isHealthy ? IntegrationResult.Success : IntegrationResult.Failed,
                ExecutionTimeMs = 0,
                LogDate = DateTime.UtcNow
            };

            _context.IntegrationLogs.Add(log);
            await SaveChangesAsync();
        }

        public async Task UpdateLastCommunicationAsync(Guid systemId)
        {
            var system = await GetByIdAsync(systemId);
            if (system == null)
                return;

            system.LastCommunication = DateTime.UtcNow;
            Update(system);
            await SaveChangesAsync();
        }

        // 상태 변경
        public async Task<bool> SetStatusAsync(Guid systemId, IntegrationStatus status)
        {
            var system = await GetByIdAsync(systemId);
            if (system == null)
                return false;

            system.Status = status;
            Update(system);
            await SaveChangesAsync();

            // 로그 추가
            var log = new IntegrationLog
            {
                SystemIntegrationId = systemId,
                Action = "StatusChange",
                Result = IntegrationResult.Success,
                ExecutionTimeMs = 0,
                LogDate = DateTime.UtcNow,
                Metadata = System.Text.Json.JsonSerializer.SerializeToDocument(new { NewStatus = status.ToString() })
            };

            _context.IntegrationLogs.Add(log);
            await SaveChangesAsync();

            return true;
        }

        // 시스템별 로그 조회
        public async Task<IEnumerable<IntegrationLog>> GetRecentLogsAsync(Guid systemId, int count = 100)
        {
            return await _context.IntegrationLogs
                .Where(l => l.SystemIntegrationId == systemId)
                .OrderByDescending(l => l.LogDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<IntegrationLog>> GetErrorLogsAsync(Guid systemId, DateTime since)
        {
            return await _context.IntegrationLogs
                .Where(l => l.SystemIntegrationId == systemId &&
                           l.Result == IntegrationResult.Failed &&
                           l.LogDate >= since)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();
        }

        // 통계
        public async Task<Dictionary<string, int>> GetCommunicationStatisticsAsync(DateTime since)
        {
            var stats = await _context.IntegrationLogs
                .Where(l => l.LogDate >= since)
                .GroupBy(l => l.SystemIntegration.SystemCode)
                .Select(g => new
                {
                    SystemCode = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.SystemCode, x => x.Count);

            return stats;
        }

        public async Task<double> GetSuccessRateAsync(Guid systemId, DateTime since)
        {
            var logs = await _context.IntegrationLogs
                .Where(l => l.SystemIntegrationId == systemId && l.LogDate >= since)
                .Select(l => l.Result)
                .ToListAsync();

            if (!logs.Any())
                return 0;

            var successCount = logs.Count(r => r == IntegrationResult.Success);
            return (double)successCount / logs.Count * 100;
        }
    }
}