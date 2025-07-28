using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BODA.FMS.MES.Data.Repositories
{
    /// <summary>
    /// Unit of Work 구현체
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MesDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        // Repository instances
        private IProductRepository? _products;
        private IWorkScenarioRepository? _workScenarios;
        private IWorkOrderRepository? _workOrders;
        private IWorkOrderExecutionRepository? _workOrderExecutions;
        private ISystemIntegrationRepository? _systemIntegrations;
        private IExecutionLogRepository? _executionLogs;

        public UnitOfWork(MesDbContext context)
        {
            _context = context;
        }

        // Lazy loading of repositories
        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        public IWorkScenarioRepository WorkScenarios =>
            _workScenarios ??= new WorkScenarioRepository(_context);

        public IWorkOrderRepository WorkOrders =>
            _workOrders ??= new WorkOrderRepository(_context);

        public IWorkOrderExecutionRepository WorkOrderExecutions =>
            _workOrderExecutions ??= new WorkOrderExecutionRepository(_context);

        public ISystemIntegrationRepository SystemIntegrations =>
            _systemIntegrations ??= new SystemIntegrationRepository(_context);

        public IExecutionLogRepository ExecutionLogs =>
            _executionLogs ??= new ExecutionLogRepository(_context);

        // Transaction Management
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // 로그 추가
                await ExecutionLogs.AddErrorLogAsync(
                    "데이터베이스 업데이트 실패",
                    "DatabaseError",
                    ex
                );
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already started.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction started.");
            }

            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction started.");
            }

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        // Database Operations
        public async Task<bool> CanConnectAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task MigrateAsync()
        {
            await _context.Database.MigrateAsync();
        }

        // Dispose pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}