// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>user_external_logins</c>.
/// </summary>
public sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("user_external_logins", tb =>
        {
            tb.HasComment("Liên kết đăng nhập ngoài (Google, Microsoft, Zalo, Apple) với tài khoản nội bộ");
            tb.HasCheckConstraint("user_external_logins_provider_check", "provider IN ('GOOGLE','MICROSOFT','APPLE','ZALO','LDAP','SAML','OIDC')");
        });

        builder.HasKey(x => x.Id).HasName("user_external_logins_pkey");

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
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Nhà cung cấp định danh");
        builder.Property(x => x.ProviderKey)
            .HasColumnName("provider_key")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Id user phía provider (sub)");
        builder.Property(x => x.ProviderEmail)
            .HasColumnName("provider_email")
            .HasColumnType("citext")
            .HasColumnOrder(5)
            .HasComment("Email phía provider");
        builder.Property(x => x.LinkedAt)
            .HasColumnName("linked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm liên kết");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .HasComment("Lần dùng gần nhất");
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
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (users.id)");
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

        builder.HasIndex(x => new { x.Provider, x.ProviderKey }, "uq_external_login").IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserExternalLogins)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_external_logins_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
