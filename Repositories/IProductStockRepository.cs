using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// ProductStock Repository 인터페이스
    /// </summary>
    public interface IProductStockRepository : IBaseRepository<ProductStock>
    {
        // 재고 번호로 조회
        Task<ProductStock?> GetByStockNumberAsync(string stockNumber);

        // 상세 정보 포함 조회
        Task<ProductStock?> GetWithDetailsAsync(Guid id);
        Task<IEnumerable<ProductStock>> GetByProductAsync(Guid productId);

        // 팔레트별 조회
        Task<IEnumerable<ProductStock>> GetByPalletAsync(int palletId);
        Task<ProductStock?> GetByPalletPositionAsync(int palletId, int position);

        // 위치별 조회
        Task<IEnumerable<ProductStock>> GetByLocationAsync(int locationId);
        Task<IEnumerable<ProductStock>> GetAvailableStocksByLocationAsync(int locationId);

        // 상태별 조회
        Task<IEnumerable<ProductStock>> GetByStatusAsync(StockStatus status);
        Task<IEnumerable<ProductStock>> GetAvailableStocksAsync();

        // FIFO 조회
        Task<ProductStock?> GetOldestAvailableStockAsync(Guid productId);
        Task<IEnumerable<ProductStock>> GetAvailableStocksFIFOAsync(Guid productId, int count);

        // 품질 상태별 조회
        Task<IEnumerable<ProductStock>> GetByQualityStatusAsync(QualityStatus qualityStatus);
        Task<IEnumerable<ProductStock>> GetDefectiveStocksAsync();

        // LOT별 조회
        Task<IEnumerable<ProductStock>> GetByLotNumberAsync(string lotNumber);

        // 재고 상태 변경
        Task<bool> UpdateStockStatusAsync(Guid stockId, StockStatus newStatus);
        Task<bool> UpdateQualityStatusAsync(Guid stockId, QualityStatus newStatus);
        Task<bool> UpdateLocationAsync(Guid stockId, int newLocationId);

        // 입출고
        Task<ProductStock> InboundStockAsync(Product product, int palletId, int position, int locationId, string stockNumber);
        Task<bool> OutboundStockAsync(Guid stockId);

        // 예약
        Task<bool> ReserveStockAsync(Guid stockId);
        Task<bool> ReleaseReservationAsync(Guid stockId);

        // 통계
        Task<Dictionary<StockStatus, int>> GetStockStatusStatisticsAsync();
        Task<Dictionary<ProductType, int>> GetStockByProductTypeAsync();
        Task<int> GetAvailableStockCountAsync(Guid productId);

        // 재고 현황
        Task<IEnumerable<StockSummary>> GetStockSummaryByProductAsync();
        Task<IEnumerable<StockSummary>> GetStockSummaryByLocationAsync();
    }

    /// <summary>
    /// 재고 요약 DTO
    /// </summary>
    public class StockSummary
    {
        public string GroupKey { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int AvailableCount { get; set; }
        public int ReservedCount { get; set; }
        public int InProcessCount { get; set; }
        public int DefectiveCount { get; set; }
    }
}