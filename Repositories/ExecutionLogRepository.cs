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
    /// ExecutionLog Repository 구현체
    /// </summary>
    public class ExecutionLogRepository : BaseRepository<ExecutionLog>, IExecutionLogRepository
    {
        public ExecutionLogRepository(MesDbContext context) : base(context)
        {
        }

        // 작업별 로그 조회
        public async Task<IEnumerable<ExecutionLog>> GetByWorkOrderAsync(Guid workOrderId, LogLevel? minLevel = null)
        {
            var query = _dbSet.Where(l => l.WorkOrderId == workOrderId);

            if (minLevel.HasValue)
            {
                query = query.Where(l => l.LogLevel >= minLevel.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExecutionLog>> GetByExecutionAsync(Guid executionId, LogLevel? minLevel = null)
        {
            var query = _dbSet.Where(l => l.WorkOrderExecutionId == executionId);

            if (minLevel.HasValue)
            {
                query = query.Where(l => l.LogLevel >= minLevel.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        // 시간대별 조회
        public async Task<IEnumerable<ExecutionLog>> GetLogsInDateRangeAsync(DateTime startDate, DateTime endDate, LogLevel? minLevel = null)
        {
            var query = _dbSet.Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

            if (minLevel.HasValue)
            {
                query = query.Where(l => l.LogLevel >= minLevel.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExecutionLog>> GetRecentLogsAsync(int count = 100, LogLevel? minLevel = null)
        {
            var query = _dbSet.AsQueryable();

            if (minLevel.HasValue)
            {
                query = query.Where(l => l.LogLevel >= minLevel.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        // 레벨별 조회
        public async Task<IEnumerable<ExecutionLog>> GetByLogLevelAsync(LogLevel level, DateTime? since = null)
        {
            var query = _dbSet.Where(l => l.LogLevel == level);

            if (since.HasValue)
            {
                query = query.Where(l => l.Timestamp >= since.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExecutionLog>> GetErrorLogsAsync(DateTime? since = null)
        {
            var query = _dbSet.Where(l => l.LogLevel >= LogLevel.Error);

            if (since.HasValue)
            {
                query = query.Where(l => l.Timestamp >= since.Value);
            }

            return await query
                .Include(l => l.WorkOrder)
                .Include(l => l.WorkOrderExecution)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExecutionLog>> GetCriticalLogsAsync(DateTime? since = null)
        {
            var query = _dbSet.Where(l => l.LogLevel == LogLevel.Critical);

            if (since.HasValue)
            {
                query = query.Where(l => l.Timestamp >= since.Value);
            }

            return await query
                .Include(l => l.WorkOrder)
                .Include(l => l.WorkOrderExecution)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        // 카테고리별 조회
        public async Task<IEnumerable<ExecutionLog>> GetByCategoryAsync(string category, DateTime? since = null)
        {
            var query = _dbSet.Where(l => l.Category == category);

            if (since.HasValue)
            {
                query = query.Where(l => l.Timestamp >= since.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        // 이벤트 타입별 조회
        public async Task<IEnumerable<ExecutionLog>> GetByEventTypeAsync(string eventType, DateTime? since = null)
        {
            var query = _dbSet.Where(l => l.EventType == eventType);

            if (since.HasValue)
            {
                query = query.Where(l => l.Timestamp >= since.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        // 시스템별 조회
        public async Task<IEnumerable<ExecutionLog>> GetBySourceSystemAsync(string sourceSystem, DateTime? since = null)
        {
            var query = _dbSet.Where(l => l.SourceSystem == sourceSystem);

            if (since.HasValue)
            {
                query = query.Where(l => l.Timestamp >= since.Value);
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExecutionLog>> GetBySystemsAsync(string sourceSystem, string targetSystem)
        {
            return await _dbSet
                .Where(l => l.SourceSystem == sourceSystem && l.TargetSystem == targetSystem)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        // 상관관계별 조회
        public async Task<IEnumerable<ExecutionLog>> GetByCorrelationIdAsync(string correlationId)
        {
            return await _dbSet
                .Where(l => l.CorrelationId == correlationId)
                .OrderBy(l => l.Timestamp)
                .ToListAsync();
        }

        // 로그 추가 헬퍼
        public async Task AddInfoLogAsync(string message, string eventType, Guid? workOrderId = null, JsonDocument? data = null)
        {
            var log = ExecutionLog.Info(message, eventType, workOrderId);
            if (data != null)
            {
                log.AdditionalData = data;
            }

            await AddAsync(log);
            await SaveChangesAsync();
        }

        public async Task AddWarningLogAsync(string message, string eventType, Guid? workOrderId = null, JsonDocument? data = null)
        {
            var log = new ExecutionLog
            {
                LogLevel = LogLevel.Warning,
                Message = message,
                EventType = eventType,
                WorkOrderId = workOrderId,
                Category = "Warning",
                AdditionalData = data
            };

            await AddAsync(log);
            await SaveChangesAsync();
        }

        public async Task AddErrorLogAsync(string message, string eventType, Exception? exception = null, Guid? workOrderId = null)
        {
            var log = ExecutionLog.Error(message, eventType, exception, workOrderId);
            await AddAsync(log);
            await SaveChangesAsync();
        }

        // 통계
        public async Task<Dictionary<LogLevel, int>> GetLogLevelStatisticsAsync(DateTime since)
        {
            return await _dbSet
                .Where(l => l.Timestamp >= since)
                .GroupBy(l => l.LogLevel)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Level, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetEventTypeStatisticsAsync(DateTime since)
        {
            return await _dbSet
                .Where(l => l.Timestamp >= since)
                .GroupBy(l => l.EventType)
                .Select(g => new { EventType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EventType, x => x.Count);
        }

        public async Task<long> GetAverageExecutionTimeAsync(string eventType, DateTime since)
        {
            var logs = await _dbSet
                .Where(l => l.EventType == eventType &&
                           l.Timestamp >= since &&
                           l.ExecutionTimeMs.HasValue)
                .Select(l => l.ExecutionTimeMs!.Value)
                .ToListAsync();

            if (!logs.Any())
                return 0;

            return (long)logs.Average();
        }

        // 정리
        public async Task<int> CleanupOldLogsAsync(DateTime olderThan, LogLevel? maxLevel = null)
        {
            var query = _dbSet.Where(l => l.Timestamp < olderThan);

            if (maxLevel.HasValue)
            {
                query = query.Where(l => l.LogLevel <= maxLevel.Value);
            }

            var logsToDelete = await query.ToListAsync();

            if (logsToDelete.Any())
            {
                _dbSet.RemoveRange(logsToDelete);
                await SaveChangesAsync();
            }

            return logsToDelete.Count;
        }
    }
}