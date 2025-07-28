using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BODA.FMS.MES.Data.Entities;

namespace BODA.FMS.MES.Data.Configurations
{
    /// <summary>
    /// BaseEntity 공통 설정
    /// </summary>
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // 기본 키 설정
            builder.HasKey(e => e.Id);

            // 기본 속성 설정
            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("생성일시");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .HasComment("수정일시");

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasComment("생성자");

            builder.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasComment("수정자");

            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("삭제 플래그");

            // 소프트 삭제 글로벌 필터
            builder.HasQueryFilter(e => !e.IsDeleted);

            // 기본 인덱스
            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName($"idx_{GetTableName(builder)}_deleted");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName($"idx_{GetTableName(builder)}_created");
        }

        protected string GetTableName(EntityTypeBuilder<TEntity> builder)
        {
            return builder.Metadata.GetTableName()?.ToLower() ?? typeof(TEntity).Name.ToLower();
        }
    }
}