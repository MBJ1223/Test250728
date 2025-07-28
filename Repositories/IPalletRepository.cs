using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Pallet Repository 인터페이스
    /// </summary>
    public interface IPalletRepository
    {
        // 기본 CRUD
        Task<Pallet?> GetByIdAsync(int id);
        Task<IEnumerable<Pallet>> GetAllAsync();
        Task<Pallet> AddAsync(Pallet entity);
        Pallet Update(Pallet entity);
        Task DeleteAsync(int id);
        Task<int> SaveChangesAsync();

        // 팔레트 코드로 조회
        Task<Pallet?> GetByCodeAsync(string palletCode);

        // 상세 정보 포함 조회
        Task<Pallet?> GetWithDetailsAsync(int id);
        Task<Pallet?> GetWithProductsAsync(int id);

        // 상태별 조회
        Task<IEnumerable<Pallet>> GetByStatusAsync(PalletStatus status);
        Task<IEnumerable<Pallet>> GetAvailablePalletsAsync();
        Task<IEnumerable<Pallet>> GetEmptyPalletsAsync();

        // 위치별 조회
        Task<IEnumerable<Pallet>> GetByLocationAsync(int locationId);
        Task<Pallet?> GetByCurrentLocationAsync(int locationId, string palletCode);

        // 팔레트 타입별 조회
        Task<IEnumerable<Pallet>> GetByTypeAsync(string palletType);

        // 적재 상태별 조회
        Task<IEnumerable<Pallet>> GetFullPalletsAsync();
        Task<IEnumerable<Pallet>> GetPartiallyLoadedPalletsAsync();

        // 팔레트 상태 변경
        Task<bool> UpdateStatusAsync(int palletId, PalletStatus newStatus);
        Task<bool> UpdateLocationAsync(int palletId, int newLocationId);
        Task<bool> UpdateProductCountAsync(int palletId, int productCount);

        // 팔레트 이동
        Task<bool> MovePalletAsync(int palletId, int targetLocationId, string? moveReason = null);
        Task<PalletLocation> RecordLocationHistoryAsync(int palletId, int locationId, string? reason = null);

        // 점검 관련
        Task<IEnumerable<Pallet>> GetPalletsNeedingInspectionAsync(int daysSinceLastInspection);
        Task<bool> UpdateInspectionDateAsync(int palletId);

        // 사용 횟수 관련
        Task<bool> IncrementUsageCountAsync(int palletId);
        Task<IEnumerable<Pallet>> GetPalletsNearMaxUsageAsync(int threshold = 100);

        // 통계
        Task<Dictionary<PalletStatus, int>> GetStatusStatisticsAsync();
        Task<int> GetAvailablePalletCountAsync(string? palletType = null);
    }
}