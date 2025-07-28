using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// ExecutionLog 엔티티 설정
    /// </summary>
    public class ExecutionLogConfiguration : BaseEntityConfiguration<ExecutionLog>
    {
        public override void Configure(EntityTypeBuilder<ExecutionLog> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("ExecutionLogs", table => table.HasComment("실행 로그 테이블"));

            // 속성 설정
            builder.Property(e => e.WorkOrderId)
                .HasComment("작업 지시 ID (선택적)");

            builder.Property(e => e.WorkOrderExecutionId)
                .HasComment("작업 실행 ID (선택적)");

            builder.Property(e => e.LogLevel)
                .ConfigureEnumAsString(20, "로그 레벨")
                .HasDefaultValue(LogLevel.Info);

            builder.Property(e => e.Category)
                .IsRequiredString(50, "로그 카테고리 (예: System, Integration, Execution)");

            builder.Property(e => e.EventType)
                .IsRequiredString(100, "이벤트 타입 (예: OrderCreated, StepStarted, AMSResponse)");

            builder.Property(e => e.Message)
                .IsRequired()
                .HasColumnType("TEXT")
                .HasComment("메시지");

            builder.Property(e => e.SourceSystem)
                .IsOptionalString(50, "소스 시스템 (예: MES, AMS, EMS)");

            builder.Property(e => e.TargetSystem)
                .IsOptionalString(50, "대상 시스템");

            builder.Property(e => e.AdditionalData)
                .ConfigureJson("JSON", "추가 데이터 (JSON)");

            builder.Property(e => e.CorrelationId)
                .IsOptionalString(100, "상관관계 ID (관련 이벤트 추적용)");

            builder.Property(e => e.ExecutionTimeMs)
                .HasComment("실행 시간 (밀리초)");

            builder.Property(e => e.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("타임스탬프");

            // 인덱스 설정
            builder.HasIndex(e => e.Timestamp)
                .HasDatabaseName("idx_executionlogs_timestamp");

            builder.HasIndex(e => e.WorkOrderId)
                .HasDatabaseName("idx_executionlogs_workorder")
                .HasFilter("WorkOrderId IS NOT NULL");

            builder.HasIndex(e => new { e.WorkOrderId, e.Timestamp })
                .HasDatabaseName("idx_executionlogs_workorder_timestamp")
                .HasFilter("WorkOrderId IS NOT NULL");

            builder.HasIndex(e => e.WorkOrderExecutionId)
                .HasDatabaseName("idx_executionlogs_execution")
                .HasFilter("WorkOrderExecutionId IS NOT NULL");

            builder.HasIndex(e => e.LogLevel)
                .HasDatabaseName("idx_executionlogs_level");

            builder.HasIndex(e => e.EventType)
                .HasDatabaseName("idx_executionlogs_eventtype");

            builder.HasIndex(e => e.Category)
                .HasDatabaseName("idx_executionlogs_category");

            builder.HasIndex(e => e.CorrelationId)
                .HasDatabaseName("idx_executionlogs_correlation")
                .HasFilter("CorrelationId IS NOT NULL");

            builder.HasIndex(e => new { e.SourceSystem, e.TargetSystem })
                .HasDatabaseName("idx_executionlogs_systems")
                .HasFilter("SourceSystem IS NOT NULL OR TargetSystem IS NOT NULL");

            // 관계 설정
            builder.HasOne(e => e.WorkOrder)
                .WithMany(w => w.Logs)
                .HasForeignKey(e => e.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.WorkOrderExecution)
                .WithMany(ex => ex.Logs)
                .HasForeignKey(e => e.WorkOrderExecutionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}