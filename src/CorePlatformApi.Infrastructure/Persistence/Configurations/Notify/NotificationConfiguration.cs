// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Notify;

/// <summary>
/// Cấu hình EF Core cho <c>notify.notifications</c>.
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", "notify", tb =>
        {
            tb.HasComment("Thông báo gốc phát sinh từ một sự kiện nghiệp vụ (một thông báo → nhiều người nhận, nhiều kênh)");
            tb.HasCheckConstraint("notifications_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
        });

        builder.HasKey(x => x.Id).HasName("notifications_pkey");

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
        builder.Property(x => x.EventTypeId)
            .HasColumnName("event_type_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Loại sự kiện (NULL = thông báo thủ công/broadcast)");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(3)
            .HasComment("Phân hệ phát sinh");
        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("Đối tượng liên quan");
        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("uuid")
            .HasColumnOrder(5)
            .HasComment("Id đối tượng liên quan");
        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'NORMAL'")
            .ValueGeneratedNever()
            .HasComment("Ưu tiên");
        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Dữ liệu biến để render template");
        builder.Property(x => x.Channels)
            .HasColumnName("channels")
            .HasColumnType("text[]")
            .HasColumnOrder(8)
            .IsRequired()
            .HasComment("Kênh sẽ gửi");
        builder.Property(x => x.ScheduledAt)
            .HasColumnName("scheduled_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Hẹn giờ gửi (NULL = gửi ngay)");
        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasColumnType("varchar(200)")
            .HasColumnOrder(10)
            .HasComment("Chống gửi trùng khi event bị phát lại");
        builder.Property(x => x.SenderUserId)
            .HasColumnName("sender_user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người gửi (thông báo thủ công)");
        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(12)
            .HasComment("Trace id liên kết request gốc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo");

        builder.HasIndex(x => new { x.TenantId, x.IdempotencyKey }, "uq_notifications_idem")
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");
        builder.HasIndex(x => new { x.EntityType, x.EntityId }, "ix_notifications_entity");

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("notifications_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.EventType)
            .WithMany()
            .HasForeignKey(x => x.EventTypeId)
            .HasConstraintName("notifications_event_type_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
