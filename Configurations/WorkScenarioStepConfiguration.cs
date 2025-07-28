using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// WorkScenarioStep 엔티티 설정
    /// </summary>
    public class WorkScenarioStepConfiguration : BaseEntityConfiguration<WorkScenarioStep>
    {
        public override void Configure(EntityTypeBuilder<WorkScenarioStep> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("WorkScenarioSteps", table => table.HasComment("작업 시나리오 단계 테이블"));

            // 속성 설정
            builder.Property(e => e.ScenarioId)
                .IsRequired()
                .HasComment("시나리오 ID");

            builder.Property(e => e.StepNumber)
                .IsRequired()
                .HasComment("단계 번호 (실행 순서)");

            builder.Property(e => e.StepName)
                .IsRequiredString(200, "단계명");

            builder.Property(e => e.StepType)
                .ConfigureEnumAsString(50, "단계 타입");

            builder.Property(e => e.TargetSystem)
                .ConfigureEnumAsString(50, "대상 시스템");

            builder.Property(e => e.TargetLocation)
                .IsOptionalString(100, "대상 위치 (예: LoadingZoneA, WeldingStation)");

            builder.Property(e => e.ActionType)
                .ConfigureEnumAsString(50, "액션 타입");

            builder.Property(e => e.NextStepCondition)
                .ConfigureEnumAsString(50, "다음 단계 진행 조건")
                .HasDefaultValue(NextStepCondition.OnComplete);

            builder.Property(e => e.EstimatedDuration)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("예상 소요 시간 (초)");

            builder.Property(e => e.TimeoutSeconds)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("타임아웃 (초) - 0이면 무제한");

            builder.Property(e => e.MaxRetryCount)
                .IsRequired()
                .HasDefaultValue(3)
                .HasComment("재시도 횟수");

            builder.Property(e => e.AllowParallelExecution)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("병렬 실행 가능 여부");

            builder.Property(e => e.IsSkippable)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("건너뛸 수 있는지 여부");

            builder.Property(e => e.Parameters)
                .ConfigureJson("JSON", "단계별 파라미터 (JSON)");

            builder.Property(e => e.ConditionExpression)
                .IsOptionalString(500, "조건식 (Decision 타입일 때 사용)");

            builder.Property(e => e.NextStepMapping)
                .HasColumnType("TEXT")
                .HasComment("다음 단계 매핑 (조건부 분기 시 사용)");

            // 인덱스 설정
            builder.HasIndex(e => new { e.ScenarioId, e.StepNumber })
                .IsUnique()
                .HasDatabaseName("idx_workscenariosteps_scenario_step");

            builder.HasIndex(e => e.StepType)
                .HasDatabaseName("idx_workscenariosteps_type");

            builder.HasIndex(e => e.TargetSystem)
                .HasDatabaseName("idx_workscenariosteps_system");

            builder.HasIndex(e => e.ActionType)
                .HasDatabaseName("idx_workscenariosteps_action");

            // 관계 설정
            builder.HasOne(e => e.Scenario)
                .WithMany(s => s.Steps)
                .HasForeignKey(e => e.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Executions)
                .WithOne(ex => ex.ScenarioStep)
                .HasForeignKey(ex => ex.ScenarioStepId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}