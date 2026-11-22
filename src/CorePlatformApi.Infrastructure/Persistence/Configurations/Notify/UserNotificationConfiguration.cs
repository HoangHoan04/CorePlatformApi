// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho <c>notify.user_notifications</c>.
/// </summary>
public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("user_notifications", "notify", tb =>
        {
            tb.HasComment("Hộp thư thông báo in-app của từng user (thanh chuông). Đã render sẵn nội dung theo ngôn ngữ user");
            tb.HasCheckConstraint("user_notifications_category_check", "category IN ('INFO','TASK','APPROVAL','WARNING','SYSTEM')");
        });

        builder.HasKey(x => x.Id).HasName("user_notifications_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.NotificationId)
            .HasColumnName("notification_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Thông báo gốc");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Tenant (denormalize)");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Người nhận");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(4)
            .HasComment("Phân hệ (lọc theo tab)");
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Tiêu đề đã render");
        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Nội dung đã render");
        builder.Property(x => x.Icon)
            .HasColumnName("icon")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(7)
            .HasComment("Icon/ảnh đại diện người gửi");
        builder.Property(x => x.ActionUrl)
            .HasColumnName("action_url")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Link khi bấm");
        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'INFO'")
            .ValueGeneratedNever()
            .HasComment("Nhóm hiển thị");
        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Đã đọc");
        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Đọc lúc");
        builder.Property(x => x.IsPinned)
            .HasColumnName("is_pinned")
            .HasColumnType("boolean")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Ghim");
        builder.Property(x => x.IsArchived)
            .HasColumnName("is_archived")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Đã lưu trữ/ẩn");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm nhận");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt }, "ix_user_notifications_inbox")
            .IsDescending(false, true)
            .HasFilter("NOT is_archived");
        builder.HasIndex(x => x.UserId, "ix_user_notifications_unread")
            .HasFilter("NOT is_read");

        builder.HasOne(x => x.Notification)
            .WithMany(p => p.UserNotifications)
            .HasForeignKey(x => x.NotificationId)
            .HasConstraintName("user_notifications_notification_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany(p => p.UserNotifications)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_notifications_user_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
