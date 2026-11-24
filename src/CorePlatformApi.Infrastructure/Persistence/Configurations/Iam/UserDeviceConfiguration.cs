// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// Cấu hình EF Core cho bảng <c>user_devices</c>.
/// </summary>
public sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("user_devices", tb =>
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
        builder.Property(x => x.DeviceKey)
            .HasColumnName("device_key")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Định danh thiết bị do client sinh và lưu (fingerprint/installation id)");
        builder.Property(x => x.DeviceName)
            .HasColumnName("device_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .HasComment("Tên thiết bị (iPhone 15 của Hoàng, Chrome - Windows)");
        builder.Property(x => x.Platform)
            .HasColumnName("platform")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Nền tảng");
        builder.Property(x => x.OsVersion)
            .HasColumnName("os_version")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(6)
            .HasComment("Phiên bản hệ điều hành");
        builder.Property(x => x.AppVersion)
            .HasColumnName("app_version")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(7)
            .HasComment("Phiên bản app");
        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .HasColumnOrder(8)
            .HasComment("User-Agent gần nhất");
        builder.Property(x => x.LastIp)
            .HasColumnName("last_ip")
            .HasColumnType("inet")
            .HasColumnOrder(9)
            .HasComment("IP gần nhất");
        builder.Property(x => x.IsTrusted)
            .HasColumnName("is_trusted")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Thiết bị tin cậy (bỏ qua 2FA)");
        builder.Property(x => x.FirstSeenAt)
            .HasColumnName("first_seen_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần đầu thấy");
        builder.Property(x => x.LastSeenAt)
            .HasColumnName("last_seen_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần cuối hoạt động");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm thu hồi");
        builder.Property(x => x.RevokedBy)
            .HasColumnName("revoked_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người thu hồi");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(17)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(19)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.UserId, x.DeviceKey }, "uq_user_devices").IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(p => p.UserDevices)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_devices_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
