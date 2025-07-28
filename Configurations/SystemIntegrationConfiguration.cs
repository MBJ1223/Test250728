using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// SystemIntegration 엔티티 설정
    /// </summary>
    public class SystemIntegrationConfiguration : BaseEntityConfiguration<SystemIntegration>
    {
        public override void Configure(EntityTypeBuilder<SystemIntegration> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("SystemIntegrations", table => table.HasComment("시스템 통합 테이블"));

            // 속성 설정
            builder.Property(e => e.SystemCode)
                .IsRequiredString(50, "시스템 코드 (AMS, EMS, TDVMS, ARMS)");

            builder.Property(e => e.SystemName)
                .IsRequiredString(100, "시스템명");

            builder.Property(e => e.IntegrationType)
                .ConfigureEnumAsString(50, "통합 타입");

            builder.Property(e => e.Endpoint)
                .IsOptionalString(500, "엔드포인트 URL");

            builder.Property(e => e.Status)
                .ConfigureEnumAsString(20, "상태")
                .HasDefaultValue(IntegrationStatus.Active);

            builder.Property(e => e.LastCommunication)
                .HasComment("마지막 통신 시간");

            builder.Property(e => e.LastHealthCheck)
                .HasComment("마지막 헬스체크 시간");

            builder.Property(e => e.IsHealthy)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("헬스체크 상태");

            builder.Property(e => e.Configuration)
                .ConfigureJson("JSON", "설정 정보 (JSON)");

            // 인덱스 설정
            builder.HasIndex(e => e.SystemCode)
                .IsUnique()
                .HasDatabaseName("idx_systemintegrations_code");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_systemintegrations_status");

            builder.HasIndex(e => e.IntegrationType)
                .HasDatabaseName("idx_systemintegrations_type");

            builder.HasIndex(e => new { e.Status, e.IsHealthy })
                .HasDatabaseName("idx_systemintegrations_status_health");

            // 관계 설정
            builder.HasMany(e => e.IntegrationLogs)
                .WithOne(l => l.SystemIntegration)
                .HasForeignKey(l => l.SystemIntegrationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}