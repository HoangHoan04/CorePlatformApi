// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>tenant_modules</c>.
/// </summary>
public sealed class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        builder.ToTable("tenant_modules", tb =>
        {
            tb.HasComment("Phân hệ được kích hoạt cho từng tenant (gói thuê bao). User chỉ vào được phân hệ tenant đã mua");
        });

        builder.HasKey(x => x.Id).HasName("tenant_modules_pkey");

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
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.ModuleId)
            .HasColumnName("module_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Phân hệ");
        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasColumnType("boolean")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang bật");
        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Ngày bắt đầu sử dụng");
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .HasComment("Ngày hết hạn (NULL = vô thời hạn)");
        builder.Property(x => x.MaxUsers)
            .HasColumnName("max_users")
            .HasColumnType("int")
            .HasColumnOrder(6)
            .HasComment("Giới hạn số user dùng phân hệ này");
        builder.Property(x => x.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Cấu hình riêng phân hệ cho tenant");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(9)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.TenantId, x.ModuleId }, "uq_tenant_modules").IsUnique();

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("tenant_modules_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .HasConstraintName("tenant_modules_module_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
