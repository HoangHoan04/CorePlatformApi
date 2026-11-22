// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho <c>notify.device_tokens</c>.
/// </summary>
public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens", "notify", tb =>
        {
            tb.HasComment("Push token của thiết bị (FCM registration token / APNs device token)");
            tb.HasCheckConstraint("device_tokens_provider_check", "provider IN ('FCM','APNS','HMS','WEB_PUSH')");
        });

        builder.HasKey(x => x.Id).HasName("device_tokens_pkey");

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
        builder.Property(x => x.DeviceId)
            .HasColumnName("device_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Thiết bị");
        builder.Property(x => x.AppCode)
            .HasColumnName("app_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("App (HRM_MOBILE, WMS_MOBILE)");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Nhà cung cấp");
        builder.Property(x => x.Token)
            .HasColumnName("token")
            .HasColumnType("text")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Token push");
        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(6)
            .HasComment("Ngôn ngữ app trên thiết bị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn hiệu lực (FCM báo UNREGISTERED → false)");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Lần gửi thành công gần nhất");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Đăng ký lúc");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Cập nhật token lúc");

        builder.HasIndex(x => x.UserId, "ix_device_tokens_user")
            .HasFilter("is_active");

        builder.HasOne(x => x.User)
            .WithMany(p => p.DeviceTokens)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("device_tokens_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Device)
            .WithMany(p => p.DeviceTokens)
            .HasForeignKey(x => x.DeviceId)
            .HasConstraintName("device_tokens_device_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
