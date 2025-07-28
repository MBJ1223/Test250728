using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Location Repository 인터페이스
    /// </summary>
    public interface ILocationRepository
    {
        // 기본 CRUD
        Task<Location?> GetByIdAsync(int id);
        Task<IEnumerable<Location>> GetAllAsync();
        Task<Location> AddAsync(Location entity);
        Location Update(Location entity);
        Task DeleteAsync(int id);
        Task<int> SaveChangesAsync();

        // 위치 코드로 조회
        Task<Location?> GetByCodeAsync(string locationCode);

        // 위치 타입별 조회
        Task<IEnumerable<Location>> GetByTypeAsync(LocationType locationType);
        Task<IEnumerable<Location>> GetActiveLocationsByTypeAsync(LocationType locationType);

        // 계층 구조 조회
        Task<Location?> GetWithSubLocationsAsync(int id);
        Task<IEnumerable<Location>> GetRootLocationsAsync();
        Task<IEnumerable<Location>> GetSubLocationsAsync(int parentId);

        // 용량 관련 조회
        Task<IEnumerable<Location>> GetAvailableLocationsAsync();
        Task<IEnumerable<Location>> GetFullLocationsAsync();
        Task<int> GetAvailableCapacityAsync(int locationId);

        // 팔레트 위치 조회
        Task<IEnumerable<Location>> GetLocationsWithPalletsAsync();
        Task<int> GetPalletCountAsync(int locationId);

        // 위치 상태 업데이트
        Task<bool> UpdateCapacityAsync(int locationId, int currentCount);
        Task<bool> ActivateLocationAsync(int locationId);
        Task<bool> DeactivateLocationAsync(int locationId);

        // 위치 검색
        Task<IEnumerable<Location>> SearchLocationsAsync(string searchTerm);

        // 좌표 기반 조회
        Task<Location?> GetNearestLocationAsync(decimal x, decimal y, decimal? z = null);
        Task<IEnumerable<Location>> GetLocationsInRangeAsync(decimal x, decimal y, decimal radius);
    }
}