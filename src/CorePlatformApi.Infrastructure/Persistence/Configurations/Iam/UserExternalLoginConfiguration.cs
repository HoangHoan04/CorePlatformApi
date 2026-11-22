// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.user_external_logins</c>.
/// </summary>
public sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("user_external_logins", "iam", tb =>
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
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("User");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Nhà cung cấp định danh");
        builder.Property(x => x.ProviderKey)
            .HasColumnName("provider_key")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Id user phía provider (sub)");
        builder.Property(x => x.ProviderEmail)
            .HasColumnName("provider_email")
            .HasColumnType("citext")
            .HasColumnOrder(4)
            .HasComment("Email phía provider");
        builder.Property(x => x.LinkedAt)
            .HasColumnName("linked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm liên kết");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Lần dùng gần nhất");

        builder.HasIndex(x => new { x.Provider, x.ProviderKey }, "uq_external_login").IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserExternalLogins)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_external_logins_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
