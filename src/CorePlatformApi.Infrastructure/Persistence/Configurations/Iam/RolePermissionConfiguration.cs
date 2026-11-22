// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.role_permissions</c>.
/// </summary>
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "iam", tb =>
        {
            tb.HasComment("Gán quyền cho vai trò kèm phạm vi dữ liệu (Data Scope) trên từng quyền");
            tb.HasCheckConstraint("role_permissions_data_scope_check", "data_scope IN ('OWN','DEPARTMENT','DEPARTMENT_TREE','BRANCH','COMPANY','ALL','CUSTOM')");
        });

        builder.HasKey(x => x.Id).HasName("role_permissions_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Vai trò");
        builder.Property(x => x.PermissionId)
            .HasColumnName("permission_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Quyền");
        builder.Property(x => x.DataScope)
            .HasColumnName("data_scope")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'OWN'")
            .ValueGeneratedNever()
            .HasComment("Phạm vi: cá nhân, phòng ban, phòng ban + cấp dưới, chi nhánh, công ty, toàn tenant, tùy chọn");
        builder.Property(x => x.Conditions)
            .HasColumnName("conditions")
            .HasColumnType("jsonb")
            .HasColumnOrder(4)
            .HasComment("Điều kiện ABAC bổ sung (VD: {\"amount_lte\": 50000000})");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.RoleId, x.PermissionId }, "uq_role_permissions").IsUnique();
        builder.HasIndex(x => x.PermissionId, "ix_role_permissions_perm");

        builder.HasOne(x => x.Role)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("role_permissions_role_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(x => x.PermissionId)
            .HasConstraintName("role_permissions_permission_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
