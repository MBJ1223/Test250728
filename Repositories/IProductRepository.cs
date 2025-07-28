using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Product Repository 인터페이스
    /// </summary>
    public interface IProductRepository : IBaseRepository<Product>
    {
        // 제품 코드로 조회
        Task<Product?> GetByCodeAsync(string productCode);

        // 활성 제품 조회
        Task<IEnumerable<Product>> GetActiveProductsAsync();

        // 제품 타입별 조회
        Task<IEnumerable<Product>> GetByTypeAsync(ProductType productType);

        // 제품 검색
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);

        // BOM 포함 제품 조회
        Task<IEnumerable<Product>> GetProductsWithBomAsync();

        // 제품 사용 여부 확인
        Task<bool> IsProductInUseAsync(Guid productId);
        Task<int> GetUsageCountAsync(Guid productId);

        // 제품 활성화/비활성화
        Task<bool> ActivateProductAsync(Guid productId);
        Task<bool> DeactivateProductAsync(Guid productId);

        // 코드 중복 확인
        Task<bool> IsCodeExistsAsync(string productCode, Guid? excludeId = null);
    }
}