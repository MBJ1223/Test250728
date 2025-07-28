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
            services.AddScoped<IProductStockRepository, ProductStockRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
            services.AddScoped<IProcessExecutionRepository, ProcessExecutionRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IPalletRepository, PalletRepository>();
            services.AddScoped<ISystemIntegrationRepository, SystemIntegrationRepository>();
            services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();
            services.AddScoped<IIntegrationLogRepository, IntegrationLogRepository>();

            // Generic Repository
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            return services;
        }
    }
}