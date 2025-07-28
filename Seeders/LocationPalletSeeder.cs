using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// 위치 및 팔레트 데이터 시더
    /// </summary>
    public class LocationPalletSeeder : IDataSeeder
    {
        private readonly MesDbContext _context;
        private readonly ILogger<LocationPalletSeeder> _logger;

        public int Order => 2; // Product 다음에 실행

        public LocationPalletSeeder(MesDbContext context, ILogger<LocationPalletSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasDataAsync()
        {
            return await _context.Locations.AnyAsync() || await _context.Pallets.AnyAsync();
        }

        public async Task SeedAsync()
        {
            if (await HasDataAsync())
            {
                _logger.LogInformation("위치 및 팔레트 데이터가 이미 존재합니다.");
                return;
            }

            _logger.LogInformation("위치 및 팔레트 데이터 시딩 시작...");

            // 위치 생성
            await CreateLocationsAsync();

            // 팔레트 생성
            await CreatePalletsAsync();

            _logger.LogInformation("위치 및 팔레트 데이터 시딩 완료");
        }

        private async Task CreateLocationsAsync()
        {
            var locations = new[]
            {
                // 입고 구역
                new Location
                {
                    LocationCode = "IB-01",
                    LocationName = "입고 구역 1",
                    LocationType = LocationType.LoadingZone,
                    MaxCapacity = 10,
                    CurrentCount = 0,
                    CoordinateX = 0,
                    CoordinateY = 0,
                    CoordinateZ = 0,
                    IsActive = true
                },
                new Location
                {
                    LocationCode = "IB-02",
                    LocationName = "입고 구역 2",
                    LocationType = LocationType.LoadingZone,
                    MaxCapacity = 10,
                    CurrentCount = 0,
                    CoordinateX = 5,
                    CoordinateY = 0,
                    CoordinateZ = 0,
                    IsActive = true
                },
                new Location
                {
                    LocationCode = "IB-03",
                    LocationName = "입고 구역 3",
                    LocationType = LocationType.LoadingZone,
                    MaxCapacity = 10,
                    CurrentCount = 0,
                    CoordinateX = 10,
                    CoordinateY = 0,
                    CoordinateZ = 0,
                    IsActive = true
                },

                // 대기 구역
                new Location
                {
                    LocationCode = "WZ-01",
                    LocationName = "용접 대기 구역",
                    LocationType = LocationType.WaitingZone,
                    MaxCapacity = 5,
                    CurrentCount = 0,
                    CoordinateX = 15,
                    CoordinateY = 10,
                    CoordinateZ = 0,
                    IsActive = true
                },
                new Location
                {
                    LocationCode = "WZ-02",
                    LocationName = "볼팅 대기 구역",
                    LocationType = LocationType.WaitingZone,
                    MaxCapacity = 5,
                    CurrentCount = 0,
                    CoordinateX = 15,
                    CoordinateY = 20,
                    CoordinateZ = 0,
                    IsActive = true
                },

                // 작업 스테이션
                new Location
                {
                    LocationCode = "WELD-01",
                    LocationName = "용접 스테이션 1",
                    LocationType = LocationType.ProductionLine,
                    MaxCapacity = 1,
                    CurrentCount = 0,
                    CoordinateX = 30,
                    CoordinateY = 10,
                    CoordinateZ = 0,
                    IsActive = true
                },
                new Location
                {
                    LocationCode = "BOLT-01",
                    LocationName = "볼팅 스테이션 1",
                    LocationType = LocationType.ProductionLine,
                    MaxCapacity = 1,
                    CurrentCount = 0,
                    CoordinateX = 30,
                    CoordinateY = 20,
                    CoordinateZ = 0,
                    IsActive = true
                },

                // 검사 구역
                new Location
                {
                    LocationCode = "QC-01",
                    LocationName = "품질 검사 구역 1",
                    LocationType = LocationType.ProductionLine,
                    MaxCapacity = 2,
                    CurrentCount = 0,
                    CoordinateX = 45,
                    CoordinateY = 10,
                    CoordinateZ = 0,
                    IsActive = true
                },
                new Location
                {
                    LocationCode = "QC-02",
                    LocationName = "품질 검사 구역 2",
                    LocationType = LocationType.ProductionLine,
                    MaxCapacity = 2,
                    CurrentCount = 0,
                    CoordinateX = 45,
                    CoordinateY = 20,
                    CoordinateZ = 0,
                    IsActive = true
                },

                // 출고 구역
                new Location
                {
                    LocationCode = "OB-01",
                    LocationName = "출고 구역 1",
                    LocationType = LocationType.UnloadingZone,
                    MaxCapacity = 20,
                    CurrentCount = 0,
                    CoordinateX = 60,
                    CoordinateY = 15,
                    CoordinateZ = 0,
                    IsActive = true
                },

                // 창고
                new Location
                {
                    LocationCode = "WH-01",
                    LocationName = "창고 구역 A",
                    LocationType = LocationType.Warehouse,
                    MaxCapacity = 100,
                    CurrentCount = 0,
                    CoordinateX = 80,
                    CoordinateY = 30,
                    CoordinateZ = 0,
                    IsActive = true
                },

                // 임시 저장소
                new Location
                {
                    LocationCode = "TMP-01",
                    LocationName = "임시 저장소 1",
                    LocationType = LocationType.TemporaryStorage,
                    MaxCapacity = 30,
                    CurrentCount = 0,
                    CoordinateX = 70,
                    CoordinateY = 40,
                    CoordinateZ = 0,
                    IsActive = true
                }
            };

            await _context.Locations.AddRangeAsync(locations);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"위치 데이터 {locations.Length}개 생성 완료");
        }

        private async Task CreatePalletsAsync()
        {
            var pallets = new[]
            {
                // 사용 가능한 팔레트
                new Pallet
                {
                    PalletCode = "PLT-001",
                    PalletType = "Standard",
                    Status = PalletStatus.Available,
                    MaxSlots = 2,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 1000,
                    CreatedAt = DateTime.UtcNow
                },
                new Pallet
                {
                    PalletCode = "PLT-002",
                    PalletType = "Standard",
                    Status = PalletStatus.Available,
                    MaxSlots = 2,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 1000,
                    CreatedAt = DateTime.UtcNow
                },
                new Pallet
                {
                    PalletCode = "PLT-003",
                    PalletType = "Standard",
                    Status = PalletStatus.Available,
                    MaxSlots = 2,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 1000,
                    CreatedAt = DateTime.UtcNow
                },
                new Pallet
                {
                    PalletCode = "PLT-004",
                    PalletType = "Standard",
                    Status = PalletStatus.Available,
                    MaxSlots = 2,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 1000,
                    CreatedAt = DateTime.UtcNow
                },
                new Pallet
                {
                    PalletCode = "PLT-005",
                    PalletType = "Standard",
                    Status = PalletStatus.Available,
                    MaxSlots = 2,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 1000,
                    CreatedAt = DateTime.UtcNow
                },
                // 대형 팔레트
                new Pallet
                {
                    PalletCode = "PLT-L001",
                    PalletType = "Large",
                    Status = PalletStatus.Available,
                    MaxSlots = 4,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 800,
                    CreatedAt = DateTime.UtcNow
                },
                new Pallet
                {
                    PalletCode = "PLT-L002",
                    PalletType = "Large",
                    Status = PalletStatus.Available,
                    MaxSlots = 4,
                    CurrentProductCount = 0,
                    UsageCount = 0,
                    MaxUsageCount = 800,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Pallets.AddRangeAsync(pallets);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"팔레트 데이터 {pallets.Length}개 생성 완료");
        }
    }
}