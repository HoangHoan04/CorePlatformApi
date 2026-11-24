// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho bảng <c>device_tokens</c>.
/// </summary>
public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens", tb =>
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
        builder.Property(x => x.DeviceId)
            .HasColumnName("device_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Thiết bị");
        builder.Property(x => x.AppCode)
            .HasColumnName("app_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("App (HRM_MOBILE, WMS_MOBILE)");
        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Nhà cung cấp");
        builder.Property(x => x.Token)
            .HasColumnName("token")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .IsRequired()
            .HasComment("Token push");
        builder.Property(x => x.Locale)
            .HasColumnName("locale")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(7)
            .HasComment("Ngôn ngữ app trên thiết bị");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Còn hiệu lực (FCM báo UNREGISTERED → false)");
        builder.Property(x => x.LastUsedAt)
            .HasColumnName("last_used_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Lần gửi thành công gần nhất");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Đăng ký lúc");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .HasComment("Cập nhật token lúc");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(13)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(16)
            .HasComment("Người xóa mềm (users.id)");

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
