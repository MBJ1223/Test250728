using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// WorkScenario 엔티티 설정
    /// </summary>
    public class WorkScenarioConfiguration : BaseEntityConfiguration<WorkScenario>
    {
        public override void Configure(EntityTypeBuilder<WorkScenario> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("WorkScenarios", table => table.HasComment("작업 시나리오 테이블"));

            // 속성 설정
            builder.Property(e => e.ScenarioCode)
                .IsRequiredString(50, "시나리오 코드 (예: SCENARIO_A, SCENARIO_B)");

            builder.Property(e => e.ScenarioName)
                .IsRequiredString(200, "시나리오명");

            builder.Property(e => e.Description)
                .HasColumnType("TEXT")
                .HasComment("시나리오 설명");

            builder.Property(e => e.Version)
                .IsRequiredString(20, "버전")
                .HasDefaultValue("1.0.0");

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("활성 상태");

            builder.Property(e => e.EstimatedDuration)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("예상 소요 시간 (분)");

            builder.Property(e => e.Parameters)
                .ConfigureJson("JSON", "시나리오 파라미터 (JSON)");

            // 인덱스 설정
            builder.HasIndex(e => e.ScenarioCode)
                .IsUnique()
                .HasDatabaseName("idx_workscenarios_code");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_workscenarios_active");

            builder.HasIndex(e => new { e.IsActive, e.Version })
                .HasDatabaseName("idx_workscenarios_active_version");

            // 관계 설정
            builder.HasMany(e => e.Steps)
                .WithOne(s => s.Scenario)
                .HasForeignKey(s => s.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.WorkOrders)
                .WithOne(w => w.Scenario)
                .HasForeignKey(w => w.ScenarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}