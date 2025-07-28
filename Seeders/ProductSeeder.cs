using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// 제품 데이터 시더
    /// </summary>
    public class ProductSeeder : IDataSeeder
    {
        private readonly MesDbContext _context;
        private readonly ILogger<ProductSeeder> _logger;

        public int Order => 1;

        public ProductSeeder(MesDbContext context, ILogger<ProductSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await _context.Products.AnyAsync();
        }

        public async Task SeedAsync()
        {
            if (await HasDataAsync())
            {
                _logger.LogInformation("제품 데이터가 이미 존재합니다.");
                return;
            }

            _logger.LogInformation("제품 데이터 시딩 시작...");

            //var products = new[]
            //{
            //    new Product
            //    {
            //        ProductCode = "DEMO-PART-001",
            //        ProductName = "데모 부품 A형",
            //        Specification = "용접 및 볼팅이 필요한 기본 부품",
            //        Unit = "EA",
            //        ProductType = ProductType.Product,
            //        StandardWorkTime = 30, // 30분
            //        IsActive = true,
            //        BomData = JsonSerializer.SerializeToDocument(new
            //        {
            //            materials = new[]
            //            {
            //                new { code = "MAT-001", name = "기본 프레임", quantity = 1 },
            //                new { code = "MAT-002", name = "연결 부품", quantity = 4 },
            //                new { code = "MAT-003", name = "볼트셋", quantity = 8 }
            //            },
            //            processes = new[]
            //            {
            //                new { sequence = 1, process = "용접", duration = 15 },
            //                new { sequence = 2, process = "볼팅", duration = 10 },
            //                new { sequence = 3, process = "검사", duration = 5 }
            //            }
            //        }),
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    new Product
            //    {
            //        ProductCode = "DEMO-PART-002",
            //        ProductName = "데모 부품 B형",
            //        Specification = "용접 전용 부품",
            //        Unit = "EA",
            //        ProductType = ProductType.Product,
            //        StandardWorkTime = 20, // 20분
            //        IsActive = true,
            //        BomData = JsonSerializer.SerializeToDocument(new
            //        {
            //            materials = new[]
            //            {
            //                new { code = "MAT-001", name = "기본 프레임", quantity = 1 },
            //                new { code = "MAT-004", name = "용접 플레이트", quantity = 2 }
            //            },
            //            processes = new[]
            //            {
            //                new { sequence = 1, process = "용접", duration = 15 },
            //                new { sequence = 2, process = "검사", duration = 5 }
            //            }
            //        }),
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    new Product
            //    {
            //        ProductCode = "DEMO-ASSY-001",
            //        ProductName = "데모 조립품",
            //        Specification = "A형과 B형을 조합한 최종 제품",
            //        Unit = "EA",
            //        ProductType = ProductType.Product,
            //        StandardWorkTime = 60, // 60분
            //        IsActive = true,
            //        BomData = JsonSerializer.SerializeToDocument(new
            //        {
            //            materials = new[]
            //            {
            //                new { code = "DEMO-PART-001", name = "데모 부품 A형", quantity = 2 },
            //                new { code = "DEMO-PART-002", name = "데모 부품 B형", quantity = 1 },
            //                new { code = "MAT-005", name = "최종 조립 키트", quantity = 1 }
            //            },
            //            processes = new[]
            //            {
            //                new { sequence = 1, process = "부품 준비", duration = 10 },
            //                new { sequence = 2, process = "조립", duration = 30 },
            //                new { sequence = 3, process = "최종 검사", duration = 20 }
            //            }
            //        }),
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    // 원자재
            //    new Product
            //    {
            //        ProductCode = "MAT-001",
            //        ProductName = "기본 프레임",
            //        Specification = "표준 철제 프레임",
            //        Unit = "EA",
            //        ProductType = ProductType.Material,
            //        StandardWorkTime = 0,
            //        IsActive = true,
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    new Product
            //    {
            //        ProductCode = "MAT-002",
            //        ProductName = "연결 부품",
            //        Specification = "프레임 연결용 부품",
            //        Unit = "EA",
            //        ProductType = ProductType.Material,
            //        StandardWorkTime = 0,
            //        IsActive = true,
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    new Product
            //    {
            //        ProductCode = "MAT-003",
            //        ProductName = "볼트셋",
            //        Specification = "M10 볼트 및 너트 세트",
            //        Unit = "SET",
            //        ProductType = ProductType.Material,
            //        StandardWorkTime = 0,
            //        IsActive = true,
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    new Product
            //    {
            //        ProductCode = "MAT-004",
            //        ProductName = "용접 플레이트",
            //        Specification = "용접용 철판",
            //        Unit = "EA",
            //        ProductType = ProductType.Material,
            //        StandardWorkTime = 0,
            //        IsActive = true,
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    },
            //    new Product
            //    {
            //        ProductCode = "MAT-005",
            //        ProductName = "최종 조립 키트",
            //        Specification = "나사, 와셔, 기타 조립 부품",
            //        Unit = "SET",
            //        ProductType = ProductType.Material,
            //        StandardWorkTime = 0,
            //        IsActive = true,
            //        CreatedBy = "System",
            //        CreatedAt = DateTime.UtcNow
            //    }
            //};

            //await _context.Products.AddRangeAsync(products);
            //await _context.SaveChangesAsync();

            //_logger.LogInformation($"제품 데이터 {products.Length}개 시딩 완료");
        }
    }
}