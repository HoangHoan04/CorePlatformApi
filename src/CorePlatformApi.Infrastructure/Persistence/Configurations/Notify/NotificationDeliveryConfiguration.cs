// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Notify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho bảng <c>deliveries</c>.
/// </summary>
public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("deliveries", tb =>
        {
            tb.HasComment("Hàng đợi/nhật ký gửi theo từng kênh (outbox): email, push, SMS... có retry");
            tb.HasCheckConstraint("deliveries_channel_check", "channel IN ('PUSH','EMAIL','SMS','ZALO','WEBHOOK')");
            tb.HasCheckConstraint("deliveries_status_check", "status IN ('PENDING','SENDING','SENT','DELIVERED','OPENED','FAILED','CANCELLED','SKIPPED')");
        });

        builder.HasKey(x => x.Id).HasName("deliveries_pkey");

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
            .HasComment("Tenant");
        builder.Property(x => x.NotificationId)
            .HasColumnName("notification_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Thông báo gốc");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Người nhận (NULL nếu gửi tới email ngoài hệ thống)");
        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Kênh");
        builder.Property(x => x.Destination)
            .HasColumnName("destination")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Địa chỉ đích: email, SĐT, device token id");
        builder.Property(x => x.TemplateId)
            .HasColumnName("template_id")
            .HasColumnType("uuid")
            .HasColumnOrder(6)
            .HasComment("Template đã dùng");
        builder.Property(x => x.ConfigId)
            .HasColumnName("config_id")
            .HasColumnType("uuid")
            .HasColumnOrder(7)
            .HasComment("Cấu hình gửi (smtp_configs / push_configs)");
        builder.Property(x => x.Subject)
            .HasColumnName("subject")
            .HasColumnType("varchar(500)")
            .HasColumnOrder(8)
            .HasComment("Tiêu đề đã render");
        builder.Property(x => x.Body)
            .HasColumnName("body")
            .HasColumnType("text")
            .HasColumnOrder(9)
            .HasComment("Nội dung đã render");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái gửi");
        builder.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .HasColumnType("smallint")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần đã thử");
        builder.Property(x => x.MaxAttempts)
            .HasColumnName("max_attempts")
            .HasColumnType("smallint")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("5")
            .ValueGeneratedNever()
            .HasComment("Số lần thử tối đa");
        builder.Property(x => x.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần thử kế tiếp (exponential backoff)");
        builder.Property(x => x.ProviderMessageId)
            .HasColumnName("provider_message_id")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(14)
            .HasComment("Message id phía provider (đối soát)");
        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text")
            .HasColumnOrder(15)
            .HasComment("Lỗi gần nhất");
        builder.Property(x => x.SentAt)
            .HasColumnName("sent_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Gửi thành công lúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(18)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(22)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(23)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => x.NextAttemptAt, "ix_deliveries_queue")
            .HasFilter("status IN ('PENDING','FAILED')");
        builder.HasIndex(x => x.NotificationId, "ix_deliveries_notification");

        builder.HasOne(x => x.Notification)
            .WithMany(p => p.NotificationDeliveries)
            .HasForeignKey(x => x.NotificationId)
            .HasConstraintName("deliveries_notification_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .HasConstraintName("deliveries_template_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
