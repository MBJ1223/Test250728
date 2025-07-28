using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// ProcessExecution 엔티티 설정
    /// </summary>
    public class ProcessExecutionConfiguration : BaseEntityConfiguration<ProcessExecution>
    {
        public override void Configure(EntityTypeBuilder<ProcessExecution> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("ProcessExecutions", table => table.HasComment("공정 실행 기록 테이블"));

            // 속성 설정
            builder.Property(e => e.WorkOrderId)
                .IsRequired()
                .HasComment("작업 지시 ID");

            builder.Property(e => e.ProductStockId)
                .IsRequired()
                .HasComment("제품 재고 ID");

            builder.Property(e => e.RecipeStepId)
                .IsRequired()
                .HasComment("레시피 단계 ID");

            builder.Property(e => e.Status)
                .ConfigureEnumAsString(20, "실행 상태")
                .HasDefaultValue(ExecutionStatus.Pending);

            builder.Property(e => e.StartLocationId)
                .HasComment("시작 위치 ID");

            builder.Property(e => e.EndLocationId)
                .HasComment("종료 위치 ID");

            builder.Property(e => e.AssignedResource)
                .IsOptionalString(100, "할당된 리소스");

            builder.Property(e => e.StartTime)
                .HasComment("시작 시간");

            builder.Property(e => e.EndTime)
                .HasComment("종료 시간");

            builder.Property(e => e.ExecutionData)
                .ConfigureJson("JSON", "실행 데이터");

            builder.Property(e => e.ResultData)
                .ConfigureJson("JSON", "결과 데이터");

            builder.Property(e => e.ErrorMessage)
                .IsOptionalString(500, "오류 메시지");

            builder.Property(e => e.RetryCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("재시도 횟수");

            // 인덱스 설정
            builder.HasIndex(e => e.WorkOrderId)
                .HasDatabaseName("idx_processexec_workorder");

            builder.HasIndex(e => e.ProductStockId)
                .HasDatabaseName("idx_processexec_stock");

            builder.HasIndex(e => e.RecipeStepId)
                .HasDatabaseName("idx_processexec_recipestep");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_processexec_status");

            builder.HasIndex(e => new { e.WorkOrderId, e.Status })
                .HasDatabaseName("idx_processexec_workorder_status");

            builder.HasIndex(e => e.AssignedResource)
                .HasDatabaseName("idx_processexec_resource")
                .HasFilter("AssignedResource IS NOT NULL");

            builder.HasIndex(e => e.StartTime)
                .HasDatabaseName("idx_processexec_starttime")
                .HasFilter("StartTime IS NOT NULL");

            // 관계 설정
            builder.HasOne(e => e.WorkOrder)
                .WithMany(w => w.ProcessExecutions)
                .HasForeignKey(e => e.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.ProductStock)
                .WithMany(s => s.ProcessExecutions)
                .HasForeignKey(e => e.ProductStockId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.RecipeStep)
                .WithMany(r => r.ProcessExecutions)
                .HasForeignKey(e => e.RecipeStepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.StartLocation)
                .WithMany()
                .HasForeignKey(e => e.StartLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EndLocation)
                .WithMany()
                .HasForeignKey(e => e.EndLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}