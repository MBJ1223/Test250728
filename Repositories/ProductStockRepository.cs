using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// ProductStock Repository 구현체
    /// </summary>
    public class ProductStockRepository : BaseRepository<ProductStock>, IProductStockRepository
    {
        public ProductStockRepository(MesDbContext context) : base(context)
        {
        }

        // 재고 번호로 조회
        public async Task<ProductStock?> GetByStockNumberAsync(string stockNumber)
        {
            return await _dbSet
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .FirstOrDefaultAsync(s => s.StockNumber == stockNumber);
        }

        // 상세 정보 포함 조회
        public async Task<ProductStock?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Product)
                    .ThenInclude(p => p.Recipe)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .Include(s => s.WorkOrders)
                .Include(s => s.ProcessExecutions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<ProductStock>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(s => s.ProductId == productId)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderBy(s => s.InboundDate)
                .ToListAsync();
        }

        // 팔레트별 조회
        public async Task<IEnumerable<ProductStock>> GetByPalletAsync(int palletId)
        {
            return await _dbSet
                .Where(s => s.PalletId == palletId)
                .Include(s => s.Product)
                .Include(s => s.CurrentLocation)
                .OrderBy(s => s.PositionOnPallet)
                .ToListAsync();
        }

        public async Task<ProductStock?> GetByPalletPositionAsync(int palletId, int position)
        {
            return await _dbSet
                .Include(s => s.Product)
                .Include(s => s.CurrentLocation)
                .FirstOrDefaultAsync(s => s.PalletId == palletId && s.PositionOnPallet == position);
        }

        // 위치별 조회
        public async Task<IEnumerable<ProductStock>> GetByLocationAsync(int locationId)
        {
            return await _dbSet
                .Where(s => s.CurrentLocationId == locationId)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .OrderBy(s => s.InboundDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductStock>> GetAvailableStocksByLocationAsync(int locationId)
        {
            return await _dbSet
                .Where(s => s.CurrentLocationId == locationId && s.Status == StockStatus.Available)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .OrderBy(s => s.InboundDate)
                .ToListAsync();
        }

        // 상태별 조회
        public async Task<IEnumerable<ProductStock>> GetByStatusAsync(StockStatus status)
        {
            return await _dbSet
                .Where(s => s.Status == status)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderBy(s => s.InboundDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductStock>> GetAvailableStocksAsync()
        {
            return await GetByStatusAsync(StockStatus.Available);
        }

        // FIFO 조회
        public async Task<ProductStock?> GetOldestAvailableStockAsync(Guid productId)
        {
            return await _dbSet
                .Where(s => s.ProductId == productId &&
                           s.Status == StockStatus.Available &&
                           s.QualityStatus == QualityStatus.Good)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderBy(s => s.InboundDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductStock>> GetAvailableStocksFIFOAsync(Guid productId, int count)
        {
            return await _dbSet
                .Where(s => s.ProductId == productId &&
                           s.Status == StockStatus.Available &&
                           s.QualityStatus == QualityStatus.Good)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderBy(s => s.InboundDate)
                .Take(count)
                .ToListAsync();
        }

        // 품질 상태별 조회
        public async Task<IEnumerable<ProductStock>> GetByQualityStatusAsync(QualityStatus qualityStatus)
        {
            return await _dbSet
                .Where(s => s.QualityStatus == qualityStatus)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductStock>> GetDefectiveStocksAsync()
        {
            return await _dbSet
                .Where(s => s.QualityStatus == QualityStatus.Defective || s.Status == StockStatus.Defective)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
        }

        // LOT별 조회
        public async Task<IEnumerable<ProductStock>> GetByLotNumberAsync(string lotNumber)
        {
            return await _dbSet
                .Where(s => s.LotNumber == lotNumber)
                .Include(s => s.Product)
                .Include(s => s.Pallet)
                .Include(s => s.CurrentLocation)
                .OrderBy(s => s.InboundDate)
                .ToListAsync();
        }

        // 재고 상태 변경
        public async Task<bool> UpdateStockStatusAsync(Guid stockId, StockStatus newStatus)
        {
            var stock = await GetByIdAsync(stockId);
            if (stock == null)
                return false;

            stock.Status = newStatus;
            Update(stock);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateQualityStatusAsync(Guid stockId, QualityStatus newStatus)
        {
            var stock = await GetByIdAsync(stockId);
            if (stock == null)
                return false;

            stock.QualityStatus = newStatus;

            // 불량품인 경우 재고 상태도 변경
            if (newStatus == QualityStatus.Defective)
            {
                stock.Status = StockStatus.Defective;
            }

            Update(stock);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateLocationAsync(Guid stockId, int newLocationId)
        {
            var stock = await GetByIdAsync(stockId);
            if (stock == null)
                return false;

            stock.CurrentLocationId = newLocationId;
            Update(stock);
            await SaveChangesAsync();

            // 팔레트 위치도 업데이트
            var pallet = await _context.Pallets.FindAsync(stock.PalletId);
            if (pallet != null)
            {
                pallet.CurrentLocationId = newLocationId;
                _context.Pallets.Update(pallet);
                await SaveChangesAsync();
            }

            return true;
        }

        // 입출고
        public async Task<ProductStock> InboundStockAsync(Product product, int palletId, int position, int locationId, string stockNumber)
        {
            var stock = new ProductStock
            {
                StockNumber = stockNumber,
                ProductId = product.Id,
                Product = product,
                PalletId = palletId,
                PositionOnPallet = position,
                CurrentLocationId = locationId,
                Status = StockStatus.Available,
                InboundDate = DateTime.UtcNow,
                QualityStatus = QualityStatus.Good,
                CreatedBy = "System"
            };

            await AddAsync(stock);

            // 팔레트 제품 수 업데이트
            var pallet = await _context.Pallets.FindAsync(palletId);
            if (pallet != null)
            {
                pallet.CurrentProductCount++;
                pallet.Status = PalletStatus.InUse;
                _context.Pallets.Update(pallet);
            }

            await SaveChangesAsync();
            return stock;
        }

        public async Task<bool> OutboundStockAsync(Guid stockId)
        {
            var stock = await GetByIdAsync(stockId);
            if (stock == null)
                return false;

            stock.Status = StockStatus.Shipped;
            stock.OutboundDate = DateTime.UtcNow;
            Update(stock);

            // 팔레트 제품 수 업데이트
            var pallet = await _context.Pallets.FindAsync(stock.PalletId);
            if (pallet != null)
            {
                pallet.CurrentProductCount--;
                if (pallet.CurrentProductCount == 0)
                {
                    pallet.Status = PalletStatus.Empty;
                }
                _context.Pallets.Update(pallet);
            }

            await SaveChangesAsync();
            return true;
        }

        // 예약
        public async Task<bool> ReserveStockAsync(Guid stockId)
        {
            var stock = await GetByIdAsync(stockId);
            if (stock == null || stock.Status != StockStatus.Available)
                return false;

            stock.Status = StockStatus.Reserved;
            Update(stock);
            await SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReleaseReservationAsync(Guid stockId)
        {
            var stock = await GetByIdAsync(stockId);
            if (stock == null || stock.Status != StockStatus.Reserved)
                return false;

            stock.Status = StockStatus.Available;
            Update(stock);
            await SaveChangesAsync();

            return true;
        }

        // 통계
        public async Task<Dictionary<StockStatus, int>> GetStockStatusStatisticsAsync()
        {
            return await _dbSet
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<ProductType, int>> GetStockByProductTypeAsync()
        {
            return await _dbSet
                .Include(s => s.Product)
                .GroupBy(s => s.Product.ProductType)
                .Select(g => new { ProductType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ProductType, x => x.Count);
        }

        public async Task<int> GetAvailableStockCountAsync(Guid productId)
        {
            return await _dbSet
                .CountAsync(s => s.ProductId == productId &&
                                s.Status == StockStatus.Available &&
                                s.QualityStatus == QualityStatus.Good);
        }

        // 재고 현황
        public async Task<IEnumerable<StockSummary>> GetStockSummaryByProductAsync()
        {
            var summaries = await _dbSet
                .Include(s => s.Product)
                .GroupBy(s => new { s.ProductId, s.Product.ProductCode, s.Product.ProductName })
                .Select(g => new StockSummary
                {
                    GroupKey = g.Key.ProductId.ToString(),
                    GroupName = $"{g.Key.ProductCode} - {g.Key.ProductName}",
                    TotalCount = g.Count(),
                    AvailableCount = g.Count(s => s.Status == StockStatus.Available),
                    ReservedCount = g.Count(s => s.Status == StockStatus.Reserved),
                    InProcessCount = g.Count(s => s.Status == StockStatus.InProcess),
                    DefectiveCount = g.Count(s => s.Status == StockStatus.Defective)
                })
                .OrderBy(s => s.GroupName)
                .ToListAsync();

            return summaries;
        }

        public async Task<IEnumerable<StockSummary>> GetStockSummaryByLocationAsync()
        {
            var summaries = await _dbSet
                .Include(s => s.CurrentLocation)
                .GroupBy(s => new { s.CurrentLocationId, s.CurrentLocation.LocationCode, s.CurrentLocation.LocationName })
                .Select(g => new StockSummary
                {
                    GroupKey = g.Key.CurrentLocationId.ToString(),
                    GroupName = $"{g.Key.LocationCode} - {g.Key.LocationName}",
                    TotalCount = g.Count(),
                    AvailableCount = g.Count(s => s.Status == StockStatus.Available),
                    ReservedCount = g.Count(s => s.Status == StockStatus.Reserved),
                    InProcessCount = g.Count(s => s.Status == StockStatus.InProcess),
                    DefectiveCount = g.Count(s => s.Status == StockStatus.Defective)
                })
                .OrderBy(s => s.GroupName)
                .ToListAsync();

            return summaries;
        }
    }
}