using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// WorkOrder 엔티티 설정
    /// </summary>
    public class WorkOrderConfiguration : BaseEntityConfiguration<WorkOrder>
    {
        public override void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("WorkOrders", table => table.HasComment("작업 지시 테이블"));

            // 속성 설정
            builder.Property(e => e.OrderNumber)
                .IsRequiredString(50, "작업 지시 번호");

            builder.Property(e => e.OrderName)
                .IsRequiredString(200, "작업 지시명");

            builder.Property(e => e.ProductStockId)
                .IsRequired()
                .HasComment("제품 재고 ID");

            builder.Property(e => e.RecipeId)
                .IsRequired()
                .HasComment("레시피 ID");

            builder.Property(e => e.Status)
                .ConfigureEnumAsString(20, "작업 지시 상태")
                .HasDefaultValue(WorkOrderStatus.Created);

            builder.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(50)
                .HasComment("우선순위 (1-100)");

            builder.Property(e => e.CurrentRecipeStep)
                .IsRequired()
                .HasDefaultValue(1)
                .HasComment("현재 레시피 단계");

            builder.Property(e => e.ScheduledStartTime)
                .HasComment("예정 시작 시간");

            builder.Property(e => e.ScheduledEndTime)
                .HasComment("예정 종료 시간");

            builder.Property(e => e.ActualStartTime)
                .HasComment("실제 시작 시간");

            builder.Property(e => e.ActualEndTime)
                .HasComment("실제 종료 시간");

            builder.Property(e => e.ProgressPercentage)
                .ConfigureDecimal(5, 2, "진행률 (0-100)")
                .HasDefaultValue(0);

            builder.Property(e => e.Parameters)
                .ConfigureJson("JSON", "작업 파라미터");

            builder.Property(e => e.Remarks)
                .HasColumnType("TEXT")
                .HasComment("비고");

            // 인덱스 설정
            builder.HasIndex(e => e.OrderNumber)
                .IsUnique()
                .HasDatabaseName("idx_workorders_number");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_workorders_status");

            builder.HasIndex(e => e.Priority)
                .HasDatabaseName("idx_workorders_priority");

            builder.HasIndex(e => new { e.Status, e.Priority })
                .HasDatabaseName("idx_workorders_status_priority")
                .HasFilter($"Status IN ('{WorkOrderStatus.Created}', '{WorkOrderStatus.Scheduled}', '{WorkOrderStatus.InProgress}')");

            builder.HasIndex(e => e.ProductStockId)
                .HasDatabaseName("idx_workorders_stock");

            builder.HasIndex(e => e.RecipeId)
                .HasDatabaseName("idx_workorders_recipe");

            builder.HasIndex(e => e.ScheduledStartTime)
                .HasDatabaseName("idx_workorders_scheduled")
                .HasFilter("ScheduledStartTime IS NOT NULL");

            builder.HasIndex(e => e.CurrentRecipeStep)
                .HasDatabaseName("idx_workorders_current_step");

            // 관계 설정
            builder.HasOne(e => e.ProductStock)
                .WithMany(s => s.WorkOrders)
                .HasForeignKey(e => e.ProductStockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Recipe)
                .WithMany(r => r.WorkOrders)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.ProcessExecutions)
                .WithOne(p => p.WorkOrder)
                .HasForeignKey(p => p.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Logs)
                .WithOne(l => l.WorkOrder)
                .HasForeignKey(l => l.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}