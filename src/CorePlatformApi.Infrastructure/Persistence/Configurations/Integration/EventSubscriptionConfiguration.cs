// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.event_subscriptions</c>.
/// </summary>
public sealed class EventSubscriptionConfiguration : IEntityTypeConfiguration<EventSubscription>
{
    public void Configure(EntityTypeBuilder<EventSubscription> builder)
    {
        builder.ToTable("event_subscriptions", "integration", tb =>
        {
            tb.HasComment("Service nào tiêu thụ event nào (bản đồ luồng dữ liệu giữa các phân hệ)");
        });

        builder.HasKey(x => x.Id).HasName("event_subscriptions_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.EventTypeCode)
            .HasColumnName("event_type_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Event");
        builder.Property(x => x.ConsumerService)
            .HasColumnName("consumer_service")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên service tiêu thụ (wms-api, accounting-api)");
        builder.Property(x => x.QueueName)
            .HasColumnName("queue_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Queue nhận");
        builder.Property(x => x.HandlerName)
            .HasColumnName("handler_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(4)
            .HasComment("Handler xử lý (tham khảo)");
        builder.Property(x => x.MaxRetries)
            .HasColumnName("max_retries")
            .HasColumnType("smallint")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("5")
            .ValueGeneratedNever()
            .HasComment("Số lần retry trước khi vào DLQ");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang hoạt động");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(8)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");

        builder.HasIndex(x => new { x.EventTypeCode, x.ConsumerService }, "uq_event_subscriptions").IsUnique();

        builder.HasOne(x => x.EventType)
            .WithMany()
            .HasForeignKey(x => x.EventTypeCode)
            .HasConstraintName("event_subscriptions_event_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
