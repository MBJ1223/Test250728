using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// RecipeStep 엔티티 설정
    /// </summary>
    public class RecipeStepConfiguration : BaseEntityConfiguration<RecipeStep>
    {
        public override void Configure(EntityTypeBuilder<RecipeStep> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("RecipeSteps", table => table.HasComment("레시피 단계 테이블"));

            // 속성 설정
            builder.Property(e => e.RecipeId)
                .IsRequired()
                .HasComment("레시피 ID");

            builder.Property(e => e.StepNumber)
                .IsRequired()
                .HasComment("단계 번호");

            builder.Property(e => e.StepName)
                .IsRequiredString(200, "단계명");

            builder.Property(e => e.ProcessType)
                .ConfigureEnumAsString(50, "공정 타입");

            builder.Property(e => e.RequiredStationType)
                .ConfigureEnumAsString(50, "필요한 스테이션 타입");

            builder.Property(e => e.ValidStartLocations)
                .IsOptionalString(500, "시작 가능 위치 (콤마 구분)");

            builder.Property(e => e.TargetLocation)
                .IsOptionalString(100, "목표 위치");

            builder.Property(e => e.EstimatedMinutes)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("예상 소요시간 (분)");

            builder.Property(e => e.TimeoutMinutes)
                .IsRequired()
                .HasDefaultValue(30)
                .HasComment("타임아웃 (분)");

            builder.Property(e => e.IsMandatory)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("필수 단계 여부");

            builder.Property(e => e.Parameters)
                .ConfigureJson("JSON", "단계별 파라미터");

            builder.Property(e => e.NextStepConditions)
                .ConfigureJson("JSON", "다음 단계 조건");

            // 인덱스 설정
            builder.HasIndex(e => new { e.RecipeId, e.StepNumber })
                .IsUnique()
                .HasDatabaseName("idx_recipesteps_recipe_step");

            builder.HasIndex(e => e.ProcessType)
                .HasDatabaseName("idx_recipesteps_processtype");

            builder.HasIndex(e => e.RequiredStationType)
                .HasDatabaseName("idx_recipesteps_stationtype")
                .HasFilter("RequiredStationType IS NOT NULL");

            builder.HasIndex(e => e.TargetLocation)
                .HasDatabaseName("idx_recipesteps_targetlocation")
                .HasFilter("TargetLocation IS NOT NULL");

            // 관계 설정
            builder.HasOne(e => e.Recipe)
                .WithMany(r => r.Steps)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.ProcessExecutions)
                .WithOne(p => p.RecipeStep)
                .HasForeignKey(p => p.RecipeStepId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}