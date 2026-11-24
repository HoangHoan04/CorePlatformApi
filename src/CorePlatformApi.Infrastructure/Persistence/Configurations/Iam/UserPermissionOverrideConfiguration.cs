// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>user_permission_overrides</c>.
/// </summary>
public sealed class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
{
    public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
    {
        builder.ToTable("user_permission_overrides", tb =>
        {
            tb.HasComment("Quyền gán trực tiếp cho user (cộng thêm hoặc loại trừ so với vai trò)");
            tb.HasCheckConstraint("user_permission_overrides_effect_check", "effect IN ('GRANT','DENY')");
            tb.HasCheckConstraint("user_permission_overrides_data_scope_check", "data_scope IN ('OWN','DEPARTMENT','DEPARTMENT_TREE','BRANCH','COMPANY','ALL')");
        });

        builder.HasKey(x => x.Id).HasName("user_permission_overrides_pkey");

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
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.PermissionId)
            .HasColumnName("permission_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Quyền");
        builder.Property(x => x.Effect)
            .HasColumnName("effect")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'GRANT'")
            .ValueGeneratedNever()
            .HasComment("GRANT = cấp thêm, DENY = chặn (ưu tiên cao nhất)");
        builder.Property(x => x.DataScope)
            .HasColumnName("data_scope")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .HasComment("Phạm vi dữ liệu khi GRANT");
        builder.Property(x => x.ValidTo)
            .HasColumnName("valid_to")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Hết hạn");
        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(7)
            .HasComment("Lý do");
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

        builder.HasIndex(x => new { x.UserId, x.PermissionId }, "uq_user_permission_overrides").IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserPermissionOverrides)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_permission_overrides_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Permission)
            .WithMany(p => p.UserPermissionOverrides)
            .HasForeignKey(x => x.PermissionId)
            .HasConstraintName("user_permission_overrides_permission_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
