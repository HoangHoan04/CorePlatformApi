// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.outbox_messages</c>.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "integration", tb =>
        {
            tb.HasComment("Transactional Outbox: event được ghi cùng transaction nghiệp vụ, worker đọc và publish lên message broker");
            tb.HasCheckConstraint("outbox_messages_status_check", "status IN ('PENDING','PUBLISHED','FAILED','DEAD')");
        });

        builder.HasKey(x => x.Id).HasName("outbox_messages_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Message id (dùng làm idempotency key phía consumer)");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .HasComment("Tenant");
        builder.Property(x => x.EventTypeCode)
            .HasColumnName("event_type_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Mã event");
        builder.Property(x => x.AggregateType)
            .HasColumnName("aggregate_type")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(3)
            .HasComment("Loại aggregate (Order, Employee)");
        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .HasColumnType("uuid")
            .HasColumnOrder(4)
            .HasComment("Id aggregate");
        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Nội dung event");
        builder.Property(x => x.Headers)
            .HasColumnName("headers")
            .HasColumnType("jsonb")
            .HasColumnOrder(6)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Header (user, correlation...)");
        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(7)
            .HasComment("Trace id");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái publish");
        builder.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần thử");
        builder.Property(x => x.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Lần thử tiếp");
        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text")
            .HasColumnOrder(11)
            .HasComment("Lỗi gần nhất");
        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm sự kiện xảy ra");
        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(13)
            .HasComment("Publish thành công lúc");

        builder.HasIndex(x => x.NextAttemptAt, "ix_outbox_pending")
            .HasFilter("status IN ('PENDING','FAILED')");
        builder.HasIndex(x => new { x.AggregateType, x.AggregateId }, "ix_outbox_aggregate");
    }
}
