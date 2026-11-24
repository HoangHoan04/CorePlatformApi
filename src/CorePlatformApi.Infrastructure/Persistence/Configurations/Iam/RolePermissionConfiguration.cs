// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>role_permissions</c>.
/// </summary>
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", tb =>
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
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Vai trò");
        builder.Property(x => x.PermissionId)
            .HasColumnName("permission_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Quyền");
        builder.Property(x => x.DataScope)
            .HasColumnName("data_scope")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'OWN'")
            .ValueGeneratedNever()
            .HasComment("Phạm vi: cá nhân, phòng ban, phòng ban + cấp dưới, chi nhánh, công ty, toàn tenant, tùy chọn");
        builder.Property(x => x.Conditions)
            .HasColumnName("conditions")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .HasComment("Điều kiện ABAC bổ sung (VD: {\"amount_lte\": 50000000})");
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
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người xóa mềm (users.id)");

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
