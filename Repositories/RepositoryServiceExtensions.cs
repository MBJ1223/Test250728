using Microsoft.Extensions.DependencyInjection;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Repository 서비스 등록 확장 메서드
    /// </summary>
    public static class RepositoryServiceExtensions
    {
        /// <summary>
        /// MES Repository 서비스 등록
        /// </summary>
        public static IServiceCollection AddMesRepositories(this IServiceCollection services)
        {
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Individual Repositories
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWorkScenarioRepository, WorkScenarioRepository>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
            services.AddScoped<IWorkOrderExecutionRepository, WorkOrderExecutionRepository>();
            services.AddScoped<ISystemIntegrationRepository, SystemIntegrationRepository>();
            services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();

            // Generic Repository
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            return services;
        }
    }
}