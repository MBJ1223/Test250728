using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// Product 엔티티 설정
    /// </summary>
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("Products", table => table.HasComment("제품 정보 테이블"));

            // 속성 설정
            builder.Property(e => e.ProductCode)
                .IsRequiredString(50, "제품 코드");

            builder.Property(e => e.ProductName)
                .IsRequiredString(200, "제품명");

            builder.Property(e => e.Specification)
                .HasColumnType("TEXT")
                .HasComment("제품 사양");

            builder.Property(e => e.Unit)
                .IsRequiredString(20, "단위")
                .HasDefaultValue("EA");

            builder.Property(e => e.ProductType)
                .ConfigureEnumAsString(50, "제품 타입 (None, WeldingProduct, BoltingProduct, WeldingMaterial, BoltingMaterial)")   
                .HasDefaultValue(ProductType.None);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("활성 상태");

            builder.Property(e => e.BomData)
                .ConfigureJson("JSON", "BOM 정보 (JSON 형태)");

            builder.Property(e => e.StandardWorkTime)
                .ConfigureDecimal(10, 2, "표준 작업 시간 (분)")
                .HasDefaultValue(0);

            // 인덱스 설정
            builder.HasIndex(e => e.ProductCode)
                .IsUnique()
                .HasDatabaseName("idx_products_code");

            builder.HasIndex(e => e.ProductType)
                .HasDatabaseName("idx_products_type");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("idx_products_active");

            builder.HasIndex(e => new { e.IsActive, e.ProductType })
                .HasDatabaseName("idx_products_active_type");
        }
    }
}