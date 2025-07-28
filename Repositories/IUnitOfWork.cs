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
        IWorkScenarioRepository WorkScenarios { get; }
        IWorkOrderRepository WorkOrders { get; }
        IWorkOrderExecutionRepository WorkOrderExecutions { get; }
        ISystemIntegrationRepository SystemIntegrations { get; }
        IExecutionLogRepository ExecutionLogs { get; }

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