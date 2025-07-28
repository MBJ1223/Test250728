using Microsoft.Extensions.DependencyInjection;

namespace BODA.FMS.MES.Data.Seeders
{
    /// <summary>
    /// Seeder 서비스 등록 확장 메서드
    /// </summary>
    public static class SeederServiceExtensions
    {
        /// <summary>
        /// MES 데이터 시더 서비스 등록
        /// </summary>
        public static IServiceCollection AddMesSeeders(this IServiceCollection services)
        {
            // 개별 시더 등록
            services.AddScoped<ProductSeeder>();
            services.AddScoped<LocationPalletSeeder>();
            services.AddScoped<SystemIntegrationSeeder>();
            // WorkScenarioSeeder 제거됨
            // services.AddScoped<SampleDataSeeder>(); // WorkScenario 의존성으로 인해 제거

            // 메인 시더 등록
            services.AddScoped<MesDataSeeder>();

            return services;
        }
    }
}