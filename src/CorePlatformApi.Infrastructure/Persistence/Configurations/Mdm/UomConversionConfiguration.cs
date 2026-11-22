// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Mdm;

/// <summary>
/// Cấu hình EF Core cho <c>mdm.uom_conversions</c>.
/// </summary>
public sealed class UomConversionConfiguration : IEntityTypeConfiguration<UomConversion>
{
    public void Configure(EntityTypeBuilder<UomConversion> builder)
    {
        builder.ToTable("uom_conversions", "mdm", tb =>
        {
            tb.HasComment("Bảng quy đổi ĐVT tổng quát giữa hai đơn vị (kể cả khác nhóm/không tỷ lệ cố định). Quy đổi riêng theo sản phẩm (1 thùng = 24 lon) đặt ở WMS");
            tb.HasCheckConstraint("uom_conversions_factor_check", "factor > 0");
            tb.HasCheckConstraint("uom_conversions_rounding_mode_check", "rounding_mode IN ('HALF_UP','UP','DOWN','NONE')");
            tb.HasCheckConstraint("ck_uom_conversion_diff", "from_uom_id <> to_uom_id");
        });

        builder.HasKey(x => x.Id).HasName("uom_conversions_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant (NULL = chuẩn hệ thống)");
        builder.Property(x => x.FromUomId)
            .HasColumnName("from_uom_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Đơn vị nguồn");
        builder.Property(x => x.ToUomId)
            .HasColumnName("to_uom_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Đơn vị đích");
        builder.Property(x => x.Factor)
            .HasColumnName("factor")
            .HasColumnType("numeric(20,8)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("1 from = factor to");
        builder.Property(x => x.RoundingMode)
            .HasColumnName("rounding_mode")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'HALF_UP'")
            .ValueGeneratedNever()
            .HasComment("Kiểu làm tròn");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.TenantId, x.FromUomId, x.ToUomId }, "uq_uom_conversions")
            .IsUnique()
            .AreNullsDistinct(false);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("uom_conversions_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.FromUom)
            .WithMany()
            .HasForeignKey(x => x.FromUomId)
            .HasConstraintName("uom_conversions_from_uom_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ToUom)
            .WithMany()
            .HasForeignKey(x => x.ToUomId)
            .HasConstraintName("uom_conversions_to_uom_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
