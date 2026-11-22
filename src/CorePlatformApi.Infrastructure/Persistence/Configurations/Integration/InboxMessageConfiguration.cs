// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.inbox_messages</c>.
/// </summary>
public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "integration", tb =>
        {
            tb.HasComment("Inbox/Idempotency: consumer ghi nhận message đã xử lý để không xử lý trùng");
            tb.HasCheckConstraint("inbox_messages_status_check", "status IN ('PROCESSING','PROCESSED','FAILED')");
        });

        builder.HasKey(x => new { x.ConsumerService, x.MessageId }).HasName("inbox_messages_pkey");

        builder.Property(x => x.ConsumerService)
            .HasColumnName("consumer_service")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(0)
            .IsRequired()
            .HasComment("Service tiêu thụ");
        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Message id");
        builder.Property(x => x.EventTypeCode)
            .HasColumnName("event_type_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .HasComment("Mã event");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasDefaultValueSql("'PROCESSED'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(4)
            .HasComment("Lỗi");
        builder.Property(x => x.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(5)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Nhận lúc");
        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(6)
            .HasComment("Xử lý xong lúc");

    }
}
