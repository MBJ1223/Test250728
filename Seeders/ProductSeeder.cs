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
    /// 제품 및 레시피 데이터 시더
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
            return await _context.Products.AnyAsync() || await _context.Recipes.AnyAsync();
        }

        public async Task SeedAsync()
        {
            if (await HasDataAsync())
            {
                _logger.LogInformation("제품 및 레시피 데이터가 이미 존재합니다.");
                return;
            }

            _logger.LogInformation("제품 및 레시피 데이터 시딩 시작...");

            // 레시피 생성
            var weldingRecipe = await CreateWeldingRecipeAsync();
            var boltingRecipe = await CreateBoltingRecipeAsync();

            // 제품 생성
            var products = new[]
            {
                new Product
                {
                    ProductCode = "WD-001",
                    ProductName = "용접 제품 A",
                    Specification = "표준 용접 제품",
                    Unit = "EA",
                    ProductType = ProductType.WeldingProduct,
                    RecipeId = weldingRecipe.Id,
                    IsActive = true,
                    AdditionalData = JsonSerializer.SerializeToDocument(new
                    {
                        weight = 2.5,
                        dimensions = new { length = 300, width = 200, height = 150 },
                        material = "Carbon Steel"
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductCode = "BT-001",
                    ProductName = "볼팅 제품 A",
                    Specification = "표준 볼팅 제품",
                    Unit = "EA",
                    ProductType = ProductType.BoltingProduct,
                    RecipeId = boltingRecipe.Id,
                    IsActive = true,
                    AdditionalData = JsonSerializer.SerializeToDocument(new
                    {
                        weight = 1.8,
                        dimensions = new { length = 250, width = 180, height = 120 },
                        boltCount = 8,
                        boltSize = "M10"
                    }),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                // 원자재
                new Product
                {
                    ProductCode = "MAT-WD-001",
                    ProductName = "용접 원자재",
                    Specification = "용접용 철판",
                    Unit = "EA",
                    ProductType = ProductType.WeldingMaterial,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    ProductCode = "MAT-BT-001",
                    ProductName = "볼팅 원자재",
                    Specification = "볼팅용 프레임",
                    Unit = "EA",
                    ProductType = ProductType.BoltingMaterial,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"제품 데이터 {products.Length}개 시딩 완료");
        }

        private async Task<Recipe> CreateWeldingRecipeAsync()
        {
            var recipe = new Recipe
            {
                RecipeCode = "RCP-WD-001",
                RecipeName = "표준 용접 공정",
                ProductType = ProductType.WeldingProduct,
                Description = "용접 제품을 위한 표준 공정",
                Version = "1.0.0",
                IsActive = true,
                TotalEstimatedMinutes = 40,
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    weldingType = "MIG",
                    temperature = 1500,
                    wireThickness = 1.2
                }),
                CreatedBy = "System"
            };

            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();

            // 레시피 단계 추가
            var steps = new[]
            {
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 1,
                    StepName = "입고 위치에서 픽업",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "IB-01,IB-02,IB-03",
                    TargetLocation = "WZ-01",
                    EstimatedMinutes = 5,
                    TimeoutMinutes = 10,
                    IsMandatory = true,
                    Parameters = JsonSerializer.SerializeToDocument(new
                    {
                        transportMode = "AMR",
                        priority = "normal"
                    })
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 2,
                    StepName = "대기 구역으로 이동",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "WZ-01",
                    TargetLocation = "WELD-01",
                    EstimatedMinutes = 3,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 3,
                    StepName = "용접 작업",
                    ProcessType = ProcessType.Process,
                    RequiredStationType = StationType.WeldingStation,
                    ValidStartLocations = "WELD-01",
                    TargetLocation = "WELD-01",
                    EstimatedMinutes = 20,
                    TimeoutMinutes = 30,
                    IsMandatory = true,
                    Parameters = JsonSerializer.SerializeToDocument(new
                    {
                        weldingProgram = "PROG_001",
                        qualityCheck = true
                    })
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 4,
                    StepName = "검사 구역으로 이동",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "WELD-01",
                    TargetLocation = "QC-01",
                    EstimatedMinutes = 5,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 5,
                    StepName = "품질 검사",
                    ProcessType = ProcessType.Inspection,
                    RequiredStationType = StationType.InspectionStation,
                    ValidStartLocations = "QC-01",
                    TargetLocation = "QC-01",
                    EstimatedMinutes = 10,
                    TimeoutMinutes = 15,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 6,
                    StepName = "출고 위치로 이동",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "QC-01",
                    TargetLocation = "OB-01",
                    EstimatedMinutes = 5,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                }
            };

            await _context.RecipeSteps.AddRangeAsync(steps);
            await _context.SaveChangesAsync();

            return recipe;
        }

        private async Task<Recipe> CreateBoltingRecipeAsync()
        {
            var recipe = new Recipe
            {
                RecipeCode = "RCP-BT-001",
                RecipeName = "표준 볼팅 공정",
                ProductType = ProductType.BoltingProduct,
                Description = "볼팅 제품을 위한 표준 공정",
                Version = "1.0.0",
                IsActive = true,
                TotalEstimatedMinutes = 35,
                Parameters = JsonSerializer.SerializeToDocument(new
                {
                    boltType = "M10",
                    torqueSpec = 45,
                    boltCount = 8
                }),
                CreatedBy = "System"
            };

            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();

            // 레시피 단계 추가
            var steps = new[]
            {
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 1,
                    StepName = "입고 위치에서 픽업",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "IB-01,IB-02,IB-03",
                    TargetLocation = "WZ-02",
                    EstimatedMinutes = 5,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 2,
                    StepName = "볼팅 스테이션으로 이동",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "WZ-02",
                    TargetLocation = "BOLT-01",
                    EstimatedMinutes = 3,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 3,
                    StepName = "볼팅 작업",
                    ProcessType = ProcessType.Process,
                    RequiredStationType = StationType.BoltingStation,
                    ValidStartLocations = "BOLT-01",
                    TargetLocation = "BOLT-01",
                    EstimatedMinutes = 15,
                    TimeoutMinutes = 25,
                    IsMandatory = true,
                    Parameters = JsonSerializer.SerializeToDocument(new
                    {
                        boltingProgram = "BOLT_PROG_001",
                        torqueVerification = true
                    })
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 4,
                    StepName = "검사 구역으로 이동",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "BOLT-01",
                    TargetLocation = "QC-02",
                    EstimatedMinutes = 5,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 5,
                    StepName = "품질 검사",
                    ProcessType = ProcessType.Inspection,
                    RequiredStationType = StationType.InspectionStation,
                    ValidStartLocations = "QC-02",
                    TargetLocation = "QC-02",
                    EstimatedMinutes = 8,
                    TimeoutMinutes = 15,
                    IsMandatory = true
                },
                new RecipeStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = 6,
                    StepName = "출고 위치로 이동",
                    ProcessType = ProcessType.Transport,
                    ValidStartLocations = "QC-02",
                    TargetLocation = "OB-01",
                    EstimatedMinutes = 5,
                    TimeoutMinutes = 10,
                    IsMandatory = true
                }
            };

            await _context.RecipeSteps.AddRangeAsync(steps);
            await _context.SaveChangesAsync();

            return recipe;
        }
    }
}