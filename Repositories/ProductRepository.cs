using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Product Repository 구현체
    /// </summary>
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(MesDbContext context) : base(context)
        {
        }

        // 제품 코드로 조회
        public async Task<Product?> GetByCodeAsync(string productCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ProductCode == productCode);
        }

        // 활성 제품 조회
        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        // 제품 타입별 조회
        public async Task<IEnumerable<Product>> GetByTypeAsync(ProductType productType)
        {
            return await _dbSet
                .Where(p => p.ProductType == productType && p.IsActive)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        // 제품 검색
        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveProductsAsync();

            searchTerm = searchTerm.ToLower();

            return await _dbSet
                .Where(p => p.IsActive &&
                           (p.ProductCode.ToLower().Contains(searchTerm) ||
                            p.ProductName.ToLower().Contains(searchTerm) ||
                            (p.Specification != null && p.Specification.ToLower().Contains(searchTerm))))
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        // BOM 포함 제품 조회
        public async Task<IEnumerable<Product>> GetProductsWithBomAsync()
        {
            return await _dbSet
                .Where(p => p.BomData != null && p.IsActive)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        // 제품 사용 여부 확인
        public async Task<bool> IsProductInUseAsync(Guid productId)
        {
            return await _context.WorkOrders
                .AnyAsync(w => w.ProductId == productId);
        }

        public async Task<int> GetUsageCountAsync(Guid productId)
        {
            return await _context.WorkOrders
                .CountAsync(w => w.ProductId == productId);
        }

        // 제품 활성화/비활성화
        public async Task<bool> ActivateProductAsync(Guid productId)
        {
            var product = await GetByIdAsync(productId);
            if (product == null)
                return false;

            product.IsActive = true;
            Update(product);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateProductAsync(Guid productId)
        {
            var product = await GetByIdAsync(productId);
            if (product == null)
                return false;

            // 사용 중인지 확인
            if (await IsProductInUseAsync(productId))
            {
                // 진행 중인 작업이 있는지 확인
                var hasActiveOrders = await _context.WorkOrders
                    .AnyAsync(w => w.ProductId == productId &&
                                  (w.Status == WorkOrderStatus.InProgress ||
                                   w.Status == WorkOrderStatus.Scheduled));

                if (hasActiveOrders)
                    return false; // 진행 중인 작업이 있으면 비활성화 불가
            }

            product.IsActive = false;
            Update(product);
            await SaveChangesAsync();
            return true;
        }

        // 코드 중복 확인
        public async Task<bool> IsCodeExistsAsync(string productCode, Guid? excludeId = null)
        {
            var query = _dbSet.Where(p => p.ProductCode == productCode);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}