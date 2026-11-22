// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.role_permission_scopes</c>.
/// </summary>
public sealed class RolePermissionScopeConfiguration : IEntityTypeConfiguration<RolePermissionScope>
{
    public void Configure(EntityTypeBuilder<RolePermissionScope> builder)
    {
        builder.ToTable("role_permission_scopes", "iam", tb =>
        {
            tb.HasComment("Danh sách đơn vị cụ thể khi data_scope = CUSTOM (VD: chỉ xem chi nhánh HN01 và HN02)");
            tb.HasCheckConstraint("role_permission_scopes_scope_type_check", "scope_type IN ('COMPANY','BRANCH','DEPARTMENT')");
        });

        builder.HasKey(x => x.Id).HasName("role_permission_scopes_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.RolePermissionId)
            .HasColumnName("role_permission_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Quyền của vai trò");
        builder.Property(x => x.ScopeType)
            .HasColumnName("scope_type")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Loại đơn vị");
        builder.Property(x => x.ScopeId)
            .HasColumnName("scope_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Id công ty / chi nhánh / phòng ban");
        builder.Property(x => x.IncludeChildren)
            .HasColumnName("include_children")
            .HasColumnType("boolean")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Bao gồm đơn vị con");
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

        builder.HasIndex(x => new { x.RolePermissionId, x.ScopeType, x.ScopeId }, "uq_role_permission_scopes").IsUnique();

        builder.HasOne(x => x.RolePermission)
            .WithMany(p => p.RolePermissionScopes)
            .HasForeignKey(x => x.RolePermissionId)
            .HasConstraintName("role_permission_scopes_role_permission_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
