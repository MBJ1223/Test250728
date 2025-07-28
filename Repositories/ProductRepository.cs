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

        public async Task<Product?> GetWithRecipeAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Recipe)
                    .ThenInclude(r => r!.Steps)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // 활성 제품 조회
        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsWithRecipeAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .Include(p => p.Recipe)
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

        public async Task<IEnumerable<Product>> GetByTypeWithRecipeAsync(ProductType productType)
        {
            return await _dbSet
                .Where(p => p.ProductType == productType && p.IsActive)
                .Include(p => p.Recipe)
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

        // 레시피별 제품 조회
        public async Task<IEnumerable<Product>> GetByRecipeAsync(Guid recipeId)
        {
            return await _dbSet
                .Where(p => p.RecipeId == recipeId)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithoutRecipeAsync()
        {
            return await _dbSet
                .Where(p => p.RecipeId == null && p.IsActive)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
        }

        // 제품 사용 여부 확인
        public async Task<bool> IsProductInUseAsync(Guid productId)
        {
            return await _context.ProductStocks
                .AnyAsync(s => s.ProductId == productId);
        }

        public async Task<int> GetStockCountAsync(Guid productId)
        {
            return await _context.ProductStocks
                .CountAsync(s => s.ProductId == productId);
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

            // 사용 가능한 재고가 있는지 확인
            var hasAvailableStock = await _context.ProductStocks
                .AnyAsync(s => s.ProductId == productId &&
                             (s.Status == StockStatus.Available ||
                              s.Status == StockStatus.Reserved ||
                              s.Status == StockStatus.InProcess));

            if (hasAvailableStock)
                return false; // 사용 가능한 재고가 있으면 비활성화 불가

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

        // 레시피 연결
        public async Task<bool> AssignRecipeAsync(Guid productId, Guid recipeId)
        {
            var product = await GetByIdAsync(productId);
            if (product == null)
                return false;

            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null || !recipe.IsActive)
                return false;

            // 제품 타입과 레시피 타입이 일치하는지 확인
            if (product.ProductType != recipe.ProductType)
                return false;

            product.RecipeId = recipeId;
            Update(product);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRecipeAsync(Guid productId)
        {
            var product = await GetByIdAsync(productId);
            if (product == null)
                return false;

            product.RecipeId = null;
            Update(product);
            await SaveChangesAsync();
            return true;
        }
    }
}