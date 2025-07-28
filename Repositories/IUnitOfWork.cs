using System;
using System.Threading.Tasks;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Unit of Work 인터페이스
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IProductRepository Products { get; }
        IProductStockRepository ProductStocks { get; }
        IRecipeRepository Recipes { get; }
        IWorkOrderRepository WorkOrders { get; }
        IProcessExecutionRepository ProcessExecutions { get; }
        ILocationRepository Locations { get; }
        IPalletRepository Pallets { get; }
        ISystemIntegrationRepository SystemIntegrations { get; }
        IExecutionLogRepository ExecutionLogs { get; }
        IIntegrationLogRepository IntegrationLogs { get; }

        // Transaction Management
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Database Operations
        Task<bool> CanConnectAsync();
        Task MigrateAsync();
    }
}