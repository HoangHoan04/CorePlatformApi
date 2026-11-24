// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Integration;
using CorePlatformApi.Domain.Entities.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Integration;

/// <summary>
/// Cấu hình EF Core cho bảng <c>webhook_endpoints</c>.
/// </summary>
public sealed class WebhookEndpointConfiguration : IEntityTypeConfiguration<WebhookEndpoint>
{
    public void Configure(EntityTypeBuilder<WebhookEndpoint> builder)
    {
        builder.ToTable("webhook_endpoints", tb =>
        {
            tb.HasComment("Endpoint webhook OUTBOUND: ERP bắn sự kiện ra hệ thống bên thứ 3");
            tb.HasCheckConstraint("webhook_endpoints_http_method_check", "http_method IN ('POST','PUT','PATCH')");
            tb.HasCheckConstraint("webhook_endpoints_retry_strategy_check", "retry_strategy IN ('FIXED','EXPONENTIAL')");
            tb.HasCheckConstraint("webhook_endpoints_status_check", "status IN ('ACTIVE','PAUSED','DISABLED')");
        });

        builder.HasKey(x => x.Id).HasName("webhook_endpoints_pkey");

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
            .IsRequired()
            .HasComment("Tenant");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Tên endpoint");
        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasColumnType("varchar(1000)")
            .HasColumnOrder(3)
            .IsRequired()
            .HasComment("URL nhận");
        builder.Property(x => x.HttpMethod)
            .HasColumnName("http_method")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(4)
            .IsRequired()
            .HasDefaultValueSql("'POST'")
            .ValueGeneratedNever()
            .HasComment("Phương thức");
        builder.Property(x => x.EventTypes)
            .HasColumnName("event_types")
            .HasColumnType("text[]")
            .HasColumnOrder(5)
            .IsRequired()
            .HasComment("Event đăng ký (integration.event_types.code), hỗ trợ wildcard 'crm.order.*'");
        builder.Property(x => x.SecretEncrypted)
            .HasColumnName("secret_encrypted")
            .HasColumnType("text")
            .HasColumnOrder(6)
            .HasComment("Secret ký HMAC-SHA256 (header X-Signature)");
        builder.Property(x => x.CustomHeaders)
            .HasColumnName("custom_headers")
            .HasColumnType("jsonb")
            .HasColumnOrder(7)
            .IsRequired()
            .HasDefaultValueSql("'{}'")
            .ValueGeneratedNever()
            .HasComment("Header bổ sung (Authorization...) — giá trị nhạy cảm nên mã hóa");
        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasDefaultValueSql("'application/json'")
            .ValueGeneratedNever()
            .HasComment("Content-Type");
        builder.Property(x => x.TimeoutSeconds)
            .HasColumnName("timeout_seconds")
            .HasColumnType("smallint")
            .HasColumnOrder(9)
            .IsRequired()
            .HasDefaultValueSql("10")
            .ValueGeneratedNever()
            .HasComment("Timeout mỗi lần gọi");
        builder.Property(x => x.MaxRetries)
            .HasColumnName("max_retries")
            .HasColumnType("smallint")
            .HasColumnOrder(10)
            .IsRequired()
            .HasDefaultValueSql("8")
            .ValueGeneratedNever()
            .HasComment("Số lần retry tối đa");
        builder.Property(x => x.RetryStrategy)
            .HasColumnName("retry_strategy")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(11)
            .IsRequired()
            .HasDefaultValueSql("'EXPONENTIAL'")
            .ValueGeneratedNever()
            .HasComment("Chiến lược retry");
        builder.Property(x => x.RetryBaseSeconds)
            .HasColumnName("retry_base_seconds")
            .HasColumnType("int")
            .HasColumnOrder(12)
            .IsRequired()
            .HasDefaultValueSql("30")
            .ValueGeneratedNever()
            .HasComment("Khoảng chờ cơ sở giữa các lần retry");
        builder.Property(x => x.FilterCondition)
            .HasColumnName("filter_condition")
            .HasColumnType("jsonb")
            .HasColumnOrder(13)
            .HasComment("Chỉ gửi khi payload thỏa điều kiện");
        builder.Property(x => x.ConsecutiveFailures)
            .HasColumnName("consecutive_failures")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .IsRequired()
            .HasDefaultValueSql("0")
            .ValueGeneratedNever()
            .HasComment("Số lần lỗi liên tiếp");
        builder.Property(x => x.AutoDisableAfter)
            .HasColumnName("auto_disable_after")
            .HasColumnType("int")
            .HasColumnOrder(15)
            .IsRequired()
            .HasDefaultValueSql("50")
            .ValueGeneratedNever()
            .HasComment("Tự tắt sau N lần lỗi liên tiếp");
        builder.Property(x => x.LastSuccessAt)
            .HasColumnName("last_success_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(16)
            .HasComment("Lần gửi thành công gần nhất");
        builder.Property(x => x.LastFailureAt)
            .HasColumnName("last_failure_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(17)
            .HasComment("Lần lỗi gần nhất");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(15)")
            .HasColumnOrder(18)
            .IsRequired()
            .HasDefaultValueSql("'ACTIVE'")
            .ValueGeneratedNever()
            .HasComment("Trạng thái");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(20)
            .HasComment("Người tạo (iam.users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người cập nhật gần nhất (iam.users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(23)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(24)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(25)
            .HasComment("Người xóa mềm (iam.users.id)");


        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("webhook_endpoints_tenant_id_fkey")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
