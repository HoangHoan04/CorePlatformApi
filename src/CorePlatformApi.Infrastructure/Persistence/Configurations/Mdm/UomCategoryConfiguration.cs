// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.uom_categories</c>.
/// </summary>
public sealed class UomCategoryConfiguration : IEntityTypeConfiguration<UomCategory>
{
    public void Configure(EntityTypeBuilder<UomCategory> builder)
    {
        builder.ToTable("uom_categories", "mdm", tb =>
        {
            tb.HasComment("Nhóm đơn vị tính (Số lượng, Khối lượng, Thể tích, Chiều dài, Diện tích, Thời gian)");
        });

        builder.HasKey(x => x.Id).HasName("uom_categories_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Mã nhóm (COUNT, WEIGHT, VOLUME, LENGTH, AREA, TIME)");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên nhóm");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => x.Code, "uom_categories_code_key").IsUnique();
    }
}
