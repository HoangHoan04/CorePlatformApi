// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho bảng <c>event_subscriptions</c>.
/// </summary>
public sealed class EventSubscriptionConfiguration : IEntityTypeConfiguration<EventSubscription>
{
    public void Configure(EntityTypeBuilder<EventSubscription> builder)
    {
        builder.ToTable("event_subscriptions", tb =>
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
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.EventTypeCode)
            .HasColumnName("event_type_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Event");
        builder.Property(x => x.ConsumerService)
            .HasColumnName("consumer_service")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Tên service tiêu thụ (wms-api, accounting-api)");
        builder.Property(x => x.QueueName)
            .HasColumnName("queue_name")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Queue nhận");
        builder.Property(x => x.HandlerName)
            .HasColumnName("handler_name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(5)
            .HasComment("Handler xử lý (tham khảo)");
        builder.Property(x => x.MaxRetries)
            .HasColumnName("max_retries")
            .HasColumnType("smallint")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("5")
            .ValueGeneratedNever()
            .HasComment("Số lần retry trước khi vào DLQ");
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("true")
            .ValueGeneratedNever()
            .HasComment("Đang hoạt động");
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
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(11)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
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

        builder.HasIndex(x => new { x.EventTypeCode, x.ConsumerService }, "uq_event_subscriptions").IsUnique();

        builder.HasOne(x => x.EventType)
            .WithMany()
            .HasForeignKey(x => x.EventTypeCode)
            .HasPrincipalKey(p => p.Code)
            .HasConstraintName("event_subscriptions_event_type_code_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
