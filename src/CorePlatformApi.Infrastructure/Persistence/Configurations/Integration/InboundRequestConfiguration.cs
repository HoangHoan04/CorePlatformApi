// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho <c>integration.inbound_requests</c>.
/// </summary>
public sealed class InboundRequestConfiguration : IEntityTypeConfiguration<InboundRequest>
{
    public void Configure(EntityTypeBuilder<InboundRequest> builder)
    {
        builder.ToTable("inbound_requests", "integration", tb =>
        {
            tb.HasComment("Nhật ký request inbound nhận được (lưu raw để đối soát, xử lý lại, chống trùng)");
            tb.HasCheckConstraint("inbound_requests_status_check", "status IN ('RECEIVED','PROCESSING','PROCESSED','FAILED','DUPLICATE','REJECTED')");
        });

        builder.HasKey(x => x.Id).HasName("inbound_requests_pkey");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasColumnOrder(0)
            .IsRequired()
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedNever()
            .HasComment("Khóa chính");
        builder.Property(x => x.SourceId)
            .HasColumnName("source_id")
            .HasColumnType("uuid")
            .HasColumnOrder(1)
            .IsRequired()
            .HasComment("Nguồn");
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .HasColumnOrder(2)
            .HasComment("Tenant");
        builder.Property(x => x.ExternalId)
            .HasColumnName("external_id")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(3)
            .HasComment("Id giao dịch phía ngoài (mã GD ngân hàng, order_sn) để chống trùng");
        builder.Property(x => x.HttpMethod)
            .HasColumnName("http_method")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .HasComment("Phương thức");
        builder.Property(x => x.Headers)
            .HasColumnName("headers")
            .HasColumnType("jsonb")
            .HasColumnOrder(5)
            .HasComment("Header nhận được");
        builder.Property(x => x.QueryString)
            .HasColumnName("query_string")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Query string (VNPay IPN dùng query)");
        builder.Property(x => x.RawBody)
            .HasColumnName("raw_body")
            .HasColumnType("text")
            .HasColumnOrder(7)
            .HasComment("Body gốc (giữ nguyên để verify chữ ký)");
        builder.Property(x => x.ParsedPayload)
            .HasColumnName("parsed_payload")
            .HasColumnType("jsonb")
            .HasColumnOrder(8)
            .HasComment("Payload đã parse/mapping");
        builder.Property(x => x.SourceIp)
            .HasColumnName("source_ip")
            .HasColumnType("inet")
            .HasColumnOrder(9)
            .HasComment("IP gửi");
        builder.Property(x => x.SignatureValid)
            .HasColumnName("signature_valid")
            .HasColumnType("boolean")
            .HasColumnOrder(10)
            .HasComment("Kết quả kiểm tra chữ ký");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'RECEIVED'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái xử lý");
        builder.Property(x => x.ResponseStatus)
            .HasColumnName("response_status")
            .HasColumnType("smallint")
            .HasColumnOrder(12)
            .HasComment("HTTP status đã trả cho bên gửi");
        builder.Property(x => x.ResponseBody)
            .HasColumnName("response_body")
            .HasColumnType("text")
            .HasColumnOrder(13)
            .HasComment("Body đã trả (VNPay yêu cầu RspCode)");
        builder.Property(x => x.EmittedMessageId)
            .HasColumnName("emitted_message_id")
            .HasColumnType("uuid")
            .HasColumnOrder(14)
            .HasComment("Event nội bộ đã phát (outbox_messages.id)");
        builder.Property(x => x.Error)
            .HasColumnName("error")
            .HasColumnType("text")
            .HasColumnOrder(15)
            .HasComment("Lỗi xử lý");
        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .HasColumnType("smallint")
            .HasColumnOrder(16)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần xử lý lại");
        builder.Property(x => x.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Nhận lúc");
        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(18)
            .HasComment("Xử lý xong lúc");

        builder.HasIndex(x => new { x.SourceId, x.ExternalId }, "uq_inbound_requests_external")
            .IsUnique()
            .HasFilter("external_id IS NOT NULL AND status <> 'DUPLICATE'");
        builder.HasIndex(x => new { x.SourceId, x.ReceivedAt }, "ix_inbound_requests_source")
            .IsDescending(false, true);

        builder.HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .HasConstraintName("inbound_requests_source_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
