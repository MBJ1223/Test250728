using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// Location 엔티티 설정
    /// </summary>
    namespace BODA.FMS.MES.Data.Configurations
    {
        /// <summary>
        /// Location 엔티티 설정
        /// </summary>
        public class LocationConfiguration : BaseEntityConfiguration<Location>
        {
            public override void Configure(EntityTypeBuilder<Location> builder)
            {
                base.Configure(builder);

                // 테이블 설정
                builder.ToTable("Locations", table => table.HasComment("위치 정보 테이블"));

                // 속성 설정
                builder.Property(e => e.LocationCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("위치 코드");

                builder.Property(e => e.LocationName)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasComment("위치 명칭");

                builder.Property(e => e.LocationType)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasComment("위치 타입");

                builder.Property(e => e.ParentLocationId)
                    .HasComment("상위 위치 ID");

                builder.Property(e => e.MaxCapacity)
                    .IsRequired()
                    .HasDefaultValue(1)
                    .HasComment("최대 팔레트 수용 용량");

                builder.Property(e => e.CurrentCount)
                    .IsRequired()
                    .HasDefaultValue(0)
                    .HasComment("현재 팔레트 수량");

                builder.Property(e => e.CoordinateX)
                    .HasPrecision(10, 2)
                    .HasComment("X 좌표");

                builder.Property(e => e.CoordinateY)
                    .HasPrecision(10, 2)
                    .HasComment("Y 좌표");

                builder.Property(e => e.CoordinateZ)
                    .HasPrecision(10, 2)
                    .HasComment("Z 좌표 (높이)");

                builder.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("활성 상태");

                // 인덱스 설정
                builder.HasIndex(e => e.LocationCode)
                    .IsUnique()
                    .HasDatabaseName("idx_locations_code");

                builder.HasIndex(e => e.LocationType)
                    .HasDatabaseName("idx_locations_type");

                builder.HasIndex(e => e.IsActive)
                    .HasDatabaseName("idx_locations_active");

                builder.HasIndex(e => e.ParentLocationId)
                    .HasDatabaseName("idx_locations_parent")
                    .HasFilter("ParentLocationId IS NOT NULL");

                // 관계 설정
                builder.HasOne(e => e.ParentLocation)
                    .WithMany(p => p.SubLocations)
                    .HasForeignKey(e => e.ParentLocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
}