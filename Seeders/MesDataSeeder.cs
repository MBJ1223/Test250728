using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// MES 데이터 시더 - 모든 시더를 관리하고 실행
    /// </summary>
    public class MesDataSeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MesDataSeeder> _logger;

        public MesDataSeeder(IServiceProvider serviceProvider, ILogger<MesDataSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 모든 시드 데이터 생성
        /// </summary>
        public async Task SeedAllAsync()
        {
            _logger.LogInformation("MES 데이터 시딩 시작...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MesDbContext>();

            try
            {
                // 데이터베이스 연결 확인
                if (!await context.Database.CanConnectAsync())
                {
                    _logger.LogError("데이터베이스에 연결할 수 없습니다.");
                    return;
                }

                // 마이그레이션 확인
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogWarning($"적용되지 않은 마이그레이션이 {pendingMigrations.Count()}개 있습니다. 마이그레이션을 먼저 실행하세요.");
                    return;
                }

                // 모든 시더 가져오기
                var seeders = GetAllSeeders(scope.ServiceProvider);

                // 순서대로 실행
                foreach (var seeder in seeders.OrderBy(s => s.Order))
                {
                    var seederType = seeder.GetType().Name;

                    try
                    {
                        if (await seeder.HasDataAsync())
                        {
                            _logger.LogInformation($"{seederType}: 이미 데이터가 존재합니다. 건너뜁니다.");
                            continue;
                        }

                        _logger.LogInformation($"{seederType}: 시딩 시작...");
                        await seeder.SeedAsync();
                        _logger.LogInformation($"{seederType}: 시딩 완료");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{seederType}: 시딩 중 오류 발생");
                        throw;
                    }
                }

                _logger.LogInformation("MES 데이터 시딩 완료!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MES 데이터 시딩 중 오류 발생");
                throw;
            }
        }

        /// <summary>
        /// 특정 시더만 실행
        /// </summary>
        public async Task SeedAsync<TSeeder>() where TSeeder : IDataSeeder
        {
            using var scope = _serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<TSeeder>();

            _logger.LogInformation($"{typeof(TSeeder).Name} 시딩 시작...");

            if (await seeder.HasDataAsync())
            {
                _logger.LogWarning($"{typeof(TSeeder).Name}: 이미 데이터가 존재합니다.");
                return;
            }

            await seeder.SeedAsync();
            _logger.LogInformation($"{typeof(TSeeder).Name} 시딩 완료");
        }

        /// <summary>
        /// 데이터베이스 초기화 (모든 데이터 삭제 후 재생성)
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            _logger.LogWarning("데이터베이스 초기화 시작 - 모든 데이터가 삭제됩니다!");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MesDbContext>();

            // 데이터베이스 재생성
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();

            // 시드 데이터 생성
            await SeedAllAsync();
        }

        /// <summary>
        /// 시딩 상태 확인
        /// </summary>
        public async Task<Dictionary<string, bool>> GetSeedingStatusAsync()
        {
            var status = new Dictionary<string, bool>();

            using var scope = _serviceProvider.CreateScope();
            var seeders = GetAllSeeders(scope.ServiceProvider);

            foreach (var seeder in seeders)
            {
                var seederName = seeder.GetType().Name;
                status[seederName] = await seeder.HasDataAsync();
            }

            return status;
        }

        private List<IDataSeeder> GetAllSeeders(IServiceProvider serviceProvider)
        {
            return new List<IDataSeeder>
            {
                serviceProvider.GetRequiredService<ProductSeeder>(),
                serviceProvider.GetRequiredService<WorkScenarioSeeder>(),
                serviceProvider.GetRequiredService<SystemIntegrationSeeder>()
                // 추가 시더가 있으면 여기에 추가
            };
        }
    }
}