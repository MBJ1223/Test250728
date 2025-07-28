using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// PalletLocation 엔티티 설정
    /// </summary>
    public class PalletLocationConfiguration : IEntityTypeConfiguration<PalletLocation>
    {
        public void Configure(EntityTypeBuilder<PalletLocation> builder)
        {
            // 테이블 설정
            builder.ToTable("PalletLocations", table => table.HasComment("팔레트 위치 이력 테이블"));

            // 기본 키
            builder.HasKey(e => e.Id);

            // 속성 설정
            builder.Property(e => e.PalletId)
                .IsRequired()
                .HasComment("팔레트 ID");

            builder.Property(e => e.LocationId)
                .IsRequired()
                .HasComment("위치 ID");

            builder.Property(e => e.EntryTime)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("입고 일시");

            builder.Property(e => e.ExitTime)
                .HasComment("출고 일시");

            builder.Property(e => e.IsCurrent)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("현재 위치 여부");

            builder.Property(e => e.MoveReason)
                .HasMaxLength(200)
                .HasComment("이동 사유");

            builder.Property(e => e.OperatorId)
                .HasMaxLength(50)
                .HasComment("작업자 ID");

            // 인덱스 설정
            builder.HasIndex(e => e.PalletId)
                .HasDatabaseName("idx_palletlocations_pallet");

            builder.HasIndex(e => e.LocationId)
                .HasDatabaseName("idx_palletlocations_location");

            builder.HasIndex(e => new { e.PalletId, e.IsCurrent })
                .HasDatabaseName("idx_palletlocations_pallet_current")
                .HasFilter("IsCurrent = 1");

            builder.HasIndex(e => e.EntryTime)
                .HasDatabaseName("idx_palletlocations_entry");

            // 관계 설정
            builder.HasOne(e => e.Pallet)
                .WithMany(p => p.LocationHistory)
                .HasForeignKey(e => e.PalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Location)
                .WithMany(l => l.PalletLocations)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}