using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// IntegrationLog 엔티티 설정
    /// </summary>
    public class IntegrationLogConfiguration : BaseEntityConfiguration<IntegrationLog>
    {
        public override void Configure(EntityTypeBuilder<IntegrationLog> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("IntegrationLogs", table => table.HasComment("통합 로그 테이블"));

            // 속성 설정
            builder.Property(e => e.SystemIntegrationId)
                .IsRequired()
                .HasComment("시스템 통합 ID");

            builder.Property(e => e.Action)
                .IsRequiredString(100, "액션 (예: SendCommand, ReceiveResponse, HealthCheck)");

            builder.Property(e => e.HttpMethod)
                .IsOptionalString(10, "HTTP 메소드 (REST API인 경우)");

            builder.Property(e => e.Endpoint)
                .IsOptionalString(500, "엔드포인트");

            builder.Property(e => e.RequestData)
                .HasColumnType("TEXT")
                .HasComment("요청 데이터");

            builder.Property(e => e.ResponseData)
                .HasColumnType("TEXT")
                .HasComment("응답 데이터");

            builder.Property(e => e.StatusCode)
                .HasComment("상태 코드 (HTTP 상태 코드 등)");

            builder.Property(e => e.Result)
                .ConfigureEnumAsString(20, "결과");

            builder.Property(e => e.ErrorMessage)
                .IsOptionalString(500, "오류 메시지");

            builder.Property(e => e.ExecutionTimeMs)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("실행 시간 (밀리초)");

            builder.Property(e => e.LogDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("로그 날짜");

            builder.Property(e => e.CorrelationId)
                .IsOptionalString(100, "상관관계 ID (요청-응답 매칭용)");

            builder.Property(e => e.WorkOrderId)
                .HasComment("작업 지시 ID (관련된 경우)");

            builder.Property(e => e.Metadata)
                .ConfigureJson("JSON", "메타데이터 (JSON)");

            // 인덱스 설정
            builder.HasIndex(e => e.LogDate)
                .HasDatabaseName("idx_integrationlogs_date");

            builder.HasIndex(e => e.SystemIntegrationId)
                .HasDatabaseName("idx_integrationlogs_system");

            builder.HasIndex(e => new { e.SystemIntegrationId, e.LogDate })
                .HasDatabaseName("idx_integrationlogs_system_date");

            builder.HasIndex(e => e.Result)
                .HasDatabaseName("idx_integrationlogs_result");

            builder.HasIndex(e => e.Action)
                .HasDatabaseName("idx_integrationlogs_action");

            builder.HasIndex(e => e.WorkOrderId)
                .HasDatabaseName("idx_integrationlogs_workorder")
                .HasFilter("WorkOrderId IS NOT NULL");

            builder.HasIndex(e => e.CorrelationId)
                .HasDatabaseName("idx_integrationlogs_correlation")
                .HasFilter("CorrelationId IS NOT NULL");

            builder.HasIndex(e => e.ExecutionTimeMs)
                .HasDatabaseName("idx_integrationlogs_exectime")
                .HasFilter("ExecutionTimeMs > 1000"); // 1초 이상 걸린 요청 추적

            // 관계 설정
            builder.HasOne(e => e.SystemIntegration)
                .WithMany(s => s.IntegrationLogs)
                .HasForeignKey(e => e.SystemIntegrationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}