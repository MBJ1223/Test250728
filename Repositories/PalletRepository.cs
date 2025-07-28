using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Pallet Repository 구현체
    /// </summary>
    public class PalletRepository : IPalletRepository
    {
        private readonly MesDbContext _context;
        private readonly DbSet<Pallet> _dbSet;

        public PalletRepository(MesDbContext context)
        {
            _context = context;
            _dbSet = context.Pallets;
        }

        // IBaseRepository 기본 구현 (Pallet은 BaseEntity가 아니므로 직접 구현)
        public async Task<Pallet?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Pallet>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<Pallet> AddAsync(Pallet entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public Pallet Update(Pallet entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await SaveChangesAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // 팔레트 코드로 조회
        public async Task<Pallet?> GetByCodeAsync(string palletCode)
        {
            return await _dbSet
                .Include(p => p.CurrentLocation)
                .FirstOrDefaultAsync(p => p.PalletCode == palletCode);
        }

        // 상세 정보 포함 조회
        public async Task<Pallet?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CurrentLocation)
                .Include(p => p.ProductStocks)
                    .ThenInclude(s => s.Product)
                .Include(p => p.LocationHistory)
                    .ThenInclude(l => l.Location)
                .Include(p => p.StatusHistory)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pallet?> GetWithProductsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.ProductStocks)
                    .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // 상태별 조회
        public async Task<IEnumerable<Pallet>> GetByStatusAsync(PalletStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .Include(p => p.CurrentLocation)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pallet>> GetAvailablePalletsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PalletStatus.Available)
                .Include(p => p.CurrentLocation)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pallet>> GetEmptyPalletsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PalletStatus.Empty || p.CurrentProductCount == 0)
                .Include(p => p.CurrentLocation)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        // 위치별 조회
        public async Task<IEnumerable<Pallet>> GetByLocationAsync(int locationId)
        {
            return await _dbSet
                .Where(p => p.CurrentLocationId == locationId)
                .Include(p => p.ProductStocks)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        public async Task<Pallet?> GetByCurrentLocationAsync(int locationId, string palletCode)
        {
            return await _dbSet
                .Where(p => p.CurrentLocationId == locationId && p.PalletCode == palletCode)
                .Include(p => p.ProductStocks)
                .FirstOrDefaultAsync();
        }

        // 팔레트 타입별 조회
        public async Task<IEnumerable<Pallet>> GetByTypeAsync(string palletType)
        {
            return await _dbSet
                .Where(p => p.PalletType == palletType)
                .Include(p => p.CurrentLocation)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        // 적재 상태별 조회
        public async Task<IEnumerable<Pallet>> GetFullPalletsAsync()
        {
            return await _dbSet
                .Where(p => p.CurrentProductCount >= p.MaxSlots)
                .Include(p => p.CurrentLocation)
                .Include(p => p.ProductStocks)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pallet>> GetPartiallyLoadedPalletsAsync()
        {
            return await _dbSet
                .Where(p => p.CurrentProductCount > 0 && p.CurrentProductCount < p.MaxSlots)
                .Include(p => p.CurrentLocation)
                .Include(p => p.ProductStocks)
                .OrderBy(p => p.PalletCode)
                .ToListAsync();
        }

        // 팔레트 상태 변경
        public async Task<bool> UpdateStatusAsync(int palletId, PalletStatus newStatus)
        {
            var pallet = await GetByIdAsync(palletId);
            if (pallet == null)
                return false;

            var oldStatus = pallet.Status;
            pallet.Status = newStatus;
            Update(pallet);

            // 상태 변경 이력 추가
            var history = new PalletHistory
            {
                PalletId = palletId,
                PreviousStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                LocationId = pallet.CurrentLocationId
            };
            _context.PalletHistories.Add(history);

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLocationAsync(int palletId, int newLocationId)
        {
            var pallet = await GetByIdAsync(palletId);
            if (pallet == null)
                return false;

            pallet.CurrentLocationId = newLocationId;
            Update(pallet);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProductCountAsync(int palletId, int productCount)
        {
            var pallet = await GetByIdAsync(palletId);
            if (pallet == null)
                return false;

            pallet.CurrentProductCount = Math.Max(0, Math.Min(productCount, pallet.MaxSlots));

            // 상태 자동 업데이트
            if (pallet.CurrentProductCount == 0)
            {
                pallet.Status = PalletStatus.Empty;
            }
            else if (pallet.Status == PalletStatus.Empty || pallet.Status == PalletStatus.Available)
            {
                pallet.Status = PalletStatus.InUse;
            }

            Update(pallet);
            await SaveChangesAsync();
            return true;
        }

        // 팔레트 이동
        public async Task<bool> MovePalletAsync(int palletId, int targetLocationId, string? moveReason = null)
        {
            var pallet = await GetByIdAsync(palletId);
            if (pallet == null)
                return false;

            var targetLocation = await _context.Locations.FindAsync(targetLocationId);
            if (targetLocation == null || !targetLocation.IsActive)
                return false;

            // 대상 위치 용량 확인
            if (targetLocation.CurrentCount >= targetLocation.MaxCapacity)
                return false;

            // 현재 위치 이력 종료
            var currentLocationHistory = await _context.PalletLocations
                .FirstOrDefaultAsync(pl => pl.PalletId == palletId && pl.IsCurrent);

            if (currentLocationHistory != null)
            {
                currentLocationHistory.ExitTime = DateTime.UtcNow;
                currentLocationHistory.IsCurrent = false;
                _context.PalletLocations.Update(currentLocationHistory);
            }

            // 새 위치 이력 추가
            await RecordLocationHistoryAsync(palletId, targetLocationId, moveReason);

            // 팔레트 위치 업데이트
            var oldLocationId = pallet.CurrentLocationId;
            pallet.CurrentLocationId = targetLocationId;
            Update(pallet);

            // 위치 카운트 업데이트
            if (oldLocationId.HasValue)
            {
                var oldLocation = await _context.Locations.FindAsync(oldLocationId.Value);
                if (oldLocation != null)
                {
                    oldLocation.CurrentCount = Math.Max(0, oldLocation.CurrentCount - 1);
                    _context.Locations.Update(oldLocation);
                }
            }

            targetLocation.CurrentCount++;
            _context.Locations.Update(targetLocation);

            await SaveChangesAsync();
            return true;
        }

        public async Task<PalletLocation> RecordLocationHistoryAsync(int palletId, int locationId, string? reason = null)
        {
            var locationHistory = new PalletLocation
            {
                PalletId = palletId,
                LocationId = locationId,
                EntryTime = DateTime.UtcNow,
                IsCurrent = true,
                MoveReason = reason
            };

            _context.PalletLocations.Add(locationHistory);
            await SaveChangesAsync();
            return locationHistory;
        }

        // 점검 관련
        public async Task<IEnumerable<Pallet>> GetPalletsNeedingInspectionAsync(int daysSinceLastInspection)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastInspection);

            return await _dbSet
                .Where(p => !p.LastInspectionDate.HasValue || p.LastInspectionDate.Value < cutoffDate)
                .Include(p => p.CurrentLocation)
                .OrderBy(p => p.LastInspectionDate ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<bool> UpdateInspectionDateAsync(int palletId)
        {
            var pallet = await GetByIdAsync(palletId);
            if (pallet == null)
                return false;

            pallet.LastInspectionDate = DateTime.UtcNow;
            Update(pallet);
            await SaveChangesAsync();
            return true;
        }

        // 사용 횟수 관련
        public async Task<bool> IncrementUsageCountAsync(int palletId)
        {
            var pallet = await GetByIdAsync(palletId);
            if (pallet == null)
                return false;

            pallet.UsageCount++;

            // 최대 사용 횟수 도달 시 상태 변경
            if (pallet.UsageCount >= pallet.MaxUsageCount)
            {
                pallet.Status = PalletStatus.Inspection;
            }

            Update(pallet);
            await SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Pallet>> GetPalletsNearMaxUsageAsync(int threshold = 100)
        {
            return await _dbSet
                .Where(p => p.MaxUsageCount - p.UsageCount <= threshold)
                .Include(p => p.CurrentLocation)
                .OrderBy(p => p.MaxUsageCount - p.UsageCount)
                .ToListAsync();
        }

        // 통계
        public async Task<Dictionary<PalletStatus, int>> GetStatusStatisticsAsync()
        {
            return await _dbSet
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<int> GetAvailablePalletCountAsync(string? palletType = null)
        {
            var query = _dbSet.Where(p => p.Status == PalletStatus.Available);

            if (!string.IsNullOrEmpty(palletType))
            {
                query = query.Where(p => p.PalletType == palletType);
            }

            return await query.CountAsync();
        }
    }
}