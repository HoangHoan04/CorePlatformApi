// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho <c>iam.user_devices</c>.
/// </summary>
public sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("user_devices", "iam", tb =>
        {
            tb.HasComment("Thiết bị đã đăng nhập của user (giới hạn thiết bị, thu hồi theo thiết bị)");
            tb.HasCheckConstraint("user_devices_platform_check", "platform IN ('WEB','ANDROID','IOS','DESKTOP','OTHER')");
            tb.HasCheckConstraint("user_devices_status_check", "status IN ('ACTIVE','REVOKED','BLOCKED')");
        });

        builder.HasKey(x => x.Id).HasName("user_devices_pkey");

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
        builder.Property(x => x.DeviceKey)
            .HasColumnName("device_key")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Định danh thiết bị do client sinh và lưu (fingerprint/installation id)");
        builder.Property(x => x.DeviceName)
            .HasColumnName("device_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .HasComment("Tên thiết bị (iPhone 15 của Hoàng, Chrome - Windows)");
        builder.Property(x => x.Platform)
            .HasColumnName("platform")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Nền tảng");
        builder.Property(x => x.OsVersion)
            .HasColumnName("os_version")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(5)
            .HasComment("Phiên bản hệ điều hành");
        builder.Property(x => x.AppVersion)
            .HasColumnName("app_version")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(6)
            .HasComment("Phiên bản app");
        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("User-Agent gần nhất");
        builder.Property(x => x.LastIp)
            .HasColumnName("last_ip")
            .HasColumnType("inet")
            .HasColumnOrder(8)
            .HasComment("IP gần nhất");
        builder.Property(x => x.IsTrusted)
            .HasColumnName("is_trusted")
            .HasColumnType("boolean")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Thiết bị tin cậy (bỏ qua 2FA)");
        builder.Property(x => x.FirstSeenAt)
            .HasColumnName("first_seen_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần đầu thấy");
        builder.Property(x => x.LastSeenAt)
            .HasColumnName("last_seen_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần cuối hoạt động");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Thời điểm thu hồi");
        builder.Property(x => x.RevokedBy)
            .HasColumnName("revoked_by")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Người thu hồi");

        builder.HasIndex(x => new { x.UserId, x.DeviceKey }, "uq_user_devices").IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserDevices)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_devices_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
