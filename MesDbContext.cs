using Microsoft.EntityFrameworkCore;
using BODA.FMS.MES.Data.Entities;
using BODA.FMS.MES.Data.Configurations;
using System.Reflection;

namespace BODA.FMS.MES.Data
{
    public class MesDbContext : DbContext
    {
        public MesDbContext(DbContextOptions<MesDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductStock> ProductStocks { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<RecipeStep> RecipeSteps { get; set; } = null!;
        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;
        public DbSet<ProcessExecution> ProcessExecutions { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<Pallet> Pallets { get; set; } = null!;
        public DbSet<PalletLocation> PalletLocations { get; set; } = null!;
        public DbSet<PalletHistory> PalletHistories { get; set; } = null!;
        public DbSet<ExecutionLog> ExecutionLogs { get; set; } = null!;
        public DbSet<SystemIntegration> SystemIntegrations { get; set; } = null!;
        public DbSet<IntegrationLog> IntegrationLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 모든 Configuration 적용
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // ExecutionLog의 WorkOrderExecution 관계 무시 (ProcessExecution으로 대체)
            modelBuilder.Entity<ExecutionLog>()
                .Ignore(e => e.WorkOrderExecution);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // GUID를 CHAR(36)로 저장
            configurationBuilder.Properties<Guid>()
                .HaveConversion<GuidToStringConverter>();
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        // CreatedAt은 수정하지 않음
                        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// GUID를 CHAR(36) 문자열로 변환하는 Converter
    /// </summary>
    public class GuidToStringConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Guid, string>
    {
        public GuidToStringConverter()
            : base(
                guid => guid.ToString(),
                str => Guid.Parse(str))
        {
        }
    }
}