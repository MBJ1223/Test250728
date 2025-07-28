using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// ProductStock 엔티티 설정
    /// </summary>
    public class ProductStockConfiguration : BaseEntityConfiguration<ProductStock>
    {
        public override void Configure(EntityTypeBuilder<ProductStock> builder)
        {
            base.Configure(builder);

            // 테이블 설정
            builder.ToTable("ProductStocks", table => table.HasComment("제품 재고 테이블"));

            // 속성 설정
            builder.Property(e => e.StockNumber)
                .IsRequiredString(50, "재고 번호 (바코드/RFID)");

            builder.Property(e => e.ProductId)
                .IsRequired()
                .HasComment("제품 ID");

            builder.Property(e => e.PalletId)
                .IsRequired()
                .HasComment("팔레트 ID");

            builder.Property(e => e.PositionOnPallet)
                .IsRequired()
                .HasComment("팔레트 내 위치 (1: 용접제품, 2: 볼팅제품)");

            builder.Property(e => e.CurrentLocationId)
                .IsRequired()
                .HasComment("현재 위치 ID");

            builder.Property(e => e.Status)
                .ConfigureEnumAsString(20, "재고 상태")
                .HasDefaultValue(StockStatus.Available);

            builder.Property(e => e.InboundDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("입고 일시");

            builder.Property(e => e.OutboundDate)
                .HasComment("출고 일시");

            builder.Property(e => e.CurrentRecipeStep)
                .HasComment("현재 레시피 단계");

            builder.Property(e => e.QualityStatus)
                .ConfigureEnumAsString(20, "품질 상태")
                .HasDefaultValue(QualityStatus.Good);

            builder.Property(e => e.LotNumber)
                .IsOptionalString(50, "LOT 번호");

            builder.Property(e => e.Remarks)
                .HasColumnType("TEXT")
                .HasComment("비고");

            // 인덱스 설정
            builder.HasIndex(e => e.StockNumber)
                .IsUnique()
                .HasDatabaseName("idx_stocks_number");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_stocks_status");

            builder.HasIndex(e => e.InboundDate)
                .HasDatabaseName("idx_stocks_inbound");

            builder.HasIndex(e => new { e.Status, e.InboundDate })
                .HasDatabaseName("idx_stocks_status_inbound");

            builder.HasIndex(e => new { e.PalletId, e.PositionOnPallet })
                .IsUnique()
                .HasDatabaseName("idx_stocks_pallet_position");

            builder.HasIndex(e => e.CurrentLocationId)
                .HasDatabaseName("idx_stocks_location");

            builder.HasIndex(e => e.ProductId)
                .HasDatabaseName("idx_stocks_product");

            builder.HasIndex(e => new { e.ProductId, e.Status, e.InboundDate })
                .HasDatabaseName("idx_stocks_product_status_inbound")
                .HasFilter("Status = 'Available'");

            // 관계 설정
            builder.HasOne(e => e.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Pallet)
                .WithMany(p => p.ProductStocks)
                .HasForeignKey(e => e.PalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.CurrentLocation)
                .WithMany()
                .HasForeignKey(e => e.CurrentLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.WorkOrders)
                .WithOne(w => w.ProductStock)
                .HasForeignKey(w => w.ProductStockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.ProcessExecutions)
                .WithOne(p => p.ProductStock)
                .HasForeignKey(p => p.ProductStockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}