using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// Recipe 엔티티 설정
    /// </summary>
    public class RecipeConfiguration : BaseEntityConfiguration<Recipe>
    {
        public override void Configure(EntityTypeBuilder<Recipe> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("Recipes", table => table.HasComment("레시피 테이블"));

            // 속성 설정
            builder.Property(e => e.RecipeCode)
                .IsRequiredString(50, "레시피 코드");

            builder.Property(e => e.RecipeName)
                .IsRequiredString(200, "레시피명");

            builder.Property(e => e.ProductType)
                .ConfigureEnumAsString(50, "제품 타입")
                .HasDefaultValue(ProductType.None);

            builder.Property(e => e.Description)
                .HasColumnType("TEXT")
                .HasComment("설명");

            builder.Property(e => e.Version)
                .IsRequiredString(20, "버전")
                .HasDefaultValue("1.0.0");

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("활성 상태");

            builder.Property(e => e.TotalEstimatedMinutes)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("총 예상 소요시간 (분)");

            builder.Property(e => e.Parameters)
                .ConfigureJson("JSON", "레시피 파라미터");

            // 인덱스 설정
            builder.HasIndex(e => e.RecipeCode)
                .IsUnique()
                .HasDatabaseName("idx_recipes_code");

            builder.HasIndex(e => e.ProductType)
                .HasDatabaseName("idx_recipes_producttype");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_recipes_active");

            builder.HasIndex(e => new { e.ProductType, e.IsActive })
                .HasDatabaseName("idx_recipes_type_active");

            // 관계 설정
            builder.HasMany(e => e.Steps)
                .WithOne(s => s.Recipe)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Products)
                .WithOne(p => p.Recipe)
                .HasForeignKey(p => p.RecipeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(e => e.WorkOrders)
                .WithOne(w => w.Recipe)
                .HasForeignKey(w => w.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}