using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Location Repository 구현체
    /// </summary>
    public class LocationRepository : ILocationRepository
    {
        private readonly MesDbContext _context;
        private readonly DbSet<Location> _dbSet;

        public LocationRepository(MesDbContext context)
        {
            _context = context;
            _dbSet = context.Locations;
        }

        // IBaseRepository 기본 구현 (Location은 BaseEntity가 아니므로 직접 구현)
        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _dbSet.Where(l => l.IsActive).ToListAsync();
        }

        public async Task<Location> AddAsync(Location entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public Location Update(Location entity)
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

        // 위치 코드로 조회
        public async Task<Location?> GetByCodeAsync(string locationCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(l => l.LocationCode == locationCode);
        }

        // 위치 타입별 조회
        public async Task<IEnumerable<Location>> GetByTypeAsync(LocationType locationType)
        {
            return await _dbSet
                .Where(l => l.LocationType == locationType)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetActiveLocationsByTypeAsync(LocationType locationType)
        {
            return await _dbSet
                .Where(l => l.LocationType == locationType && l.IsActive)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        // 계층 구조 조회
        public async Task<Location?> GetWithSubLocationsAsync(int id)
        {
            return await _dbSet
                .Include(l => l.SubLocations)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Location>> GetRootLocationsAsync()
        {
            return await _dbSet
                .Where(l => l.ParentLocationId == null && l.IsActive)
                .Include(l => l.SubLocations)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetSubLocationsAsync(int parentId)
        {
            return await _dbSet
                .Where(l => l.ParentLocationId == parentId && l.IsActive)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        // 용량 관련 조회
        public async Task<IEnumerable<Location>> GetAvailableLocationsAsync()
        {
            return await _dbSet
                .Where(l => l.IsActive && l.CurrentCount < l.MaxCapacity)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetFullLocationsAsync()
        {
            return await _dbSet
                .Where(l => l.IsActive && l.CurrentCount >= l.MaxCapacity)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<int> GetAvailableCapacityAsync(int locationId)
        {
            var location = await GetByIdAsync(locationId);
            if (location == null)
                return 0;

            return location.MaxCapacity - location.CurrentCount;
        }

        // 팔레트 위치 조회
        public async Task<IEnumerable<Location>> GetLocationsWithPalletsAsync()
        {
            return await _dbSet
                .Where(l => l.CurrentCount > 0)
                .Include(l => l.PalletLocations)
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        public async Task<int> GetPalletCountAsync(int locationId)
        {
            return await _context.PalletLocations
                .CountAsync(pl => pl.LocationId == locationId && pl.IsCurrent);
        }

        // 위치 상태 업데이트
        public async Task<bool> UpdateCapacityAsync(int locationId, int currentCount)
        {
            var location = await GetByIdAsync(locationId);
            if (location == null)
                return false;

            location.CurrentCount = Math.Max(0, Math.Min(currentCount, location.MaxCapacity));
            Update(location);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateLocationAsync(int locationId)
        {
            var location = await GetByIdAsync(locationId);
            if (location == null)
                return false;

            location.IsActive = true;
            Update(location);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateLocationAsync(int locationId)
        {
            var location = await GetByIdAsync(locationId);
            if (location == null)
                return false;

            // 현재 팔레트가 있는지 확인
            if (location.CurrentCount > 0)
                return false;

            location.IsActive = false;
            Update(location);
            await SaveChangesAsync();
            return true;
        }

        // 위치 검색
        public async Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();

            return await _dbSet
                .Where(l => l.IsActive &&
                           (l.LocationCode.ToLower().Contains(searchTerm) ||
                            l.LocationName.ToLower().Contains(searchTerm)))
                .OrderBy(l => l.LocationCode)
                .ToListAsync();
        }

        // 좌표 기반 조회
        public async Task<Location?> GetNearestLocationAsync(decimal x, decimal y, decimal? z = null)
        {
            var query = _dbSet
                .Where(l => l.IsActive && l.CoordinateX.HasValue && l.CoordinateY.HasValue);

            if (z.HasValue)
            {
                query = query.Where(l => l.CoordinateZ.HasValue);
            }

            var locations = await query.ToListAsync();

            if (!locations.Any())
                return null;

            // 거리 계산 및 정렬
            return locations
                .OrderBy(l => CalculateDistance(l, x, y, z))
                .FirstOrDefault();
        }

        public async Task<IEnumerable<Location>> GetLocationsInRangeAsync(decimal x, decimal y, decimal radius)
        {
            var query = _dbSet
                .Where(l => l.IsActive && l.CoordinateX.HasValue && l.CoordinateY.HasValue);

            var locations = await query.ToListAsync();

            return locations
                .Where(l => CalculateDistance(l, x, y) <= (double)radius)
                .OrderBy(l => CalculateDistance(l, x, y))
                .ToList();
        }

        private double CalculateDistance(Location location, decimal x, decimal y, decimal? z = null)
        {
            if (!location.CoordinateX.HasValue || !location.CoordinateY.HasValue)
                return double.MaxValue;

            var dx = (double)(location.CoordinateX.Value - x);
            var dy = (double)(location.CoordinateY.Value - y);
            var dz = 0.0;

            if (z.HasValue && location.CoordinateZ.HasValue)
            {
                dz = (double)(location.CoordinateZ.Value - z.Value);
            }

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}