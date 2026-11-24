// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho bảng <c>webhook_deliveries</c>.
/// </summary>
public sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("webhook_deliveries", tb =>
        {
            tb.HasComment("Nhật ký từng lần gửi webhook outbound (payload, response, retry)");
            tb.HasCheckConstraint("webhook_deliveries_status_check", "status IN ('PENDING','SUCCESS','FAILED','RETRYING','DEAD')");
        });

        builder.HasKey(x => x.Id).HasName("webhook_deliveries_pkey");

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
        builder.Property(x => x.EndpointId)
            .HasColumnName("endpoint_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Endpoint");
        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("Event gốc (outbox_messages.id)");
        builder.Property(x => x.EventTypeCode)
            .HasColumnName("event_type_code")
            .HasColumnType("varchar(150)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasComment("Mã event");
        builder.Property(x => x.RequestUrl)
            .HasColumnName("request_url")
            .HasColumnType("varchar(1000)")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("URL thực tế");
        builder.Property(x => x.RequestHeaders)
            .HasColumnName("request_headers")
            .HasColumnType("jsonb")
            .HasColumnOrder(6)
            .HasComment("Header đã gửi (đã che secret)");
        builder.Property(x => x.RequestBody)
            .HasColumnName("request_body")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .IsRequired()
            .HasComment("Payload đã gửi");
        builder.Property(x => x.AttemptNo)
            .HasColumnName("attempt_no")
            .HasColumnType("smallint")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("1")
            .ValueGeneratedNever()
            .HasComment("Lần thử thứ");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("'PENDING'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.ResponseStatus)
            .HasColumnName("response_status")
            .HasColumnType("smallint")
            .HasColumnOrder(10)
            .HasComment("HTTP status trả về");
        builder.Property(x => x.ResponseBody)
            .HasColumnName("response_body")
            .HasColumnType("text")
            .HasColumnOrder(11)
            .HasComment("Body trả về (cắt tối đa vài KB)");
        builder.Property(x => x.DurationMs)
            .HasColumnName("duration_ms")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .HasComment("Thời gian phản hồi");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(13)
            .HasComment("Lỗi mạng/timeout");
        builder.Property(x => x.NextRetryAt)
            .HasColumnName("next_retry_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(14)
            .HasComment("Lần retry tiếp");
        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(15)
            .HasComment("Hoàn tất lúc");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Tạo lúc");
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

        builder.HasIndex(x => new { x.EndpointId, x.CreatedAt }, "ix_webhook_deliveries_endpoint")
            .IsDescending(false, true);
        builder.HasIndex(x => x.NextRetryAt, "ix_webhook_deliveries_retry")
            .HasFilter("status = 'RETRYING'");

        builder.HasOne(x => x.Endpoint)
            .WithMany(p => p.WebhookDeliveries)
            .HasForeignKey(x => x.EndpointId)
            .HasConstraintName("webhook_deliveries_endpoint_id_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
