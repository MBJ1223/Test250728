using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// Pallet 엔티티 설정
    /// </summary>
    public class PalletConfiguration : IEntityTypeConfiguration<Pallet>
    {
        public void Configure(EntityTypeBuilder<Pallet> builder)
        {
            // 테이블 설정
            builder.ToTable("Pallets", table => table.HasComment("팔레트 테이블"));

            // 기본 키
            builder.HasKey(e => e.Id);

            // 속성 설정
            builder.Property(e => e.PalletCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("팔레트 바코드/RFID");

            builder.Property(e => e.PalletType)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Standard")
                .HasComment("팔레트 타입");

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(PalletStatus.Available)
                .HasComment("현재 상태");

            builder.Property(e => e.CurrentLocationId)
                .HasComment("현재 위치 ID");

            builder.Property(e => e.MaxSlots)
                .IsRequired()
                .HasDefaultValue(2)
                .HasComment("최대 적재 슬롯 수");

            builder.Property(e => e.CurrentProductCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("현재 적재된 제품 수");

            builder.Property(e => e.UsageCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("사용 횟수");

            builder.Property(e => e.MaxUsageCount)
                .IsRequired()
                .HasDefaultValue(1000)
                .HasComment("최대 사용 횟수");

            builder.Property(e => e.LastInspectionDate)
                .HasComment("마지막 점검일");

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("생성일시");

            builder.Property(e => e.UpdatedAt)
                .HasComment("수정일시");

            // 인덱스 설정
            builder.HasIndex(e => e.PalletCode)
                .IsUnique()
                .HasDatabaseName("idx_pallets_code");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("idx_pallets_status");

            builder.HasIndex(e => e.CurrentLocationId)
                .HasDatabaseName("idx_pallets_location")
                .HasFilter("CurrentLocationId IS NOT NULL");

            builder.HasIndex(e => e.PalletType)
                .HasDatabaseName("idx_pallets_type");

            // 관계 설정
            builder.HasOne(e => e.CurrentLocation)
                .WithMany()
                .HasForeignKey(e => e.CurrentLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.ProductStocks)
                .WithOne(p => p.Pallet)
                .HasForeignKey(p => p.PalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.LocationHistory)
                .WithOne(l => l.Pallet)
                .HasForeignKey(l => l.PalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.StatusHistory)
                .WithOne(h => h.Pallet)
                .HasForeignKey(h => h.PalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // 계산된 속성 무시
            builder.Ignore(e => e.IsFull);
            builder.Ignore(e => e.IsEmpty);
        }
    }
}