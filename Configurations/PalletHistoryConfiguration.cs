using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// PalletHistory 엔티티 설정
    /// </summary>
    public class PalletHistoryConfiguration : IEntityTypeConfiguration<PalletHistory>
    {
        public void Configure(EntityTypeBuilder<PalletHistory> builder)
        {
            // 테이블 설정
            builder.ToTable("PalletHistories", table => table.HasComment("팔레트 상태 변경 이력 테이블"));

            // 기본 키
            builder.HasKey(e => e.Id);

            // 속성 설정
            builder.Property(e => e.PalletId)
                .IsRequired()
                .HasComment("팔레트 ID");

            builder.Property(e => e.PreviousStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasComment("이전 상태");

            builder.Property(e => e.NewStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasComment("새 상태");

            builder.Property(e => e.ChangedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("변경 일시");

            builder.Property(e => e.ChangeReason)
                .HasMaxLength(200)
                .HasComment("변경 사유");

            builder.Property(e => e.OperatorId)
                .HasMaxLength(50)
                .HasComment("작업자 ID");

            builder.Property(e => e.LocationId)
                .HasComment("위치 ID (변경 시점)");

            // 인덱스 설정
            builder.HasIndex(e => e.PalletId)
                .HasDatabaseName("idx_pallethistory_pallet");

            builder.HasIndex(e => e.ChangedAt)
                .HasDatabaseName("idx_pallethistory_changed");

            builder.HasIndex(e => e.NewStatus)
                .HasDatabaseName("idx_pallethistory_status");

            // 관계 설정
            builder.HasOne(e => e.Pallet)
                .WithMany(p => p.StatusHistory)
                .HasForeignKey(e => e.PalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}