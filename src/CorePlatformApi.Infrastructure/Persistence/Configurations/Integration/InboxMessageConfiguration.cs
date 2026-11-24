// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho bảng <c>inbox_messages</c>.
/// </summary>
public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", tb =>
        {
            tb.HasComment("Inbox/Idempotency: consumer ghi nhận message đã xử lý để không xử lý trùng");
            tb.HasCheckConstraint("inbox_messages_status_check", "status IN ('PROCESSING','PROCESSED','FAILED')");
        });

        builder.HasKey(x => x.Id).HasName("inbox_messages_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính (UUID v7)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống");
        builder.Property(x => x.ConsumerService)
            .HasColumnName("consumer_service")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Service tiêu thụ");
        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("Message id");
        builder.Property(x => x.EventTypeCode)
            .HasColumnName("event_type_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .HasComment("Mã event");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("'PROCESSED'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Lỗi");
        builder.Property(x => x.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Nhận lúc");
        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(8)
            .HasComment("Xử lý xong lúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(10)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(11)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(12)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(13)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(15)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.ConsumerService, x.MessageId }, "inbox_messages_consumer_service_message_id_key").IsUnique();
    }
}
