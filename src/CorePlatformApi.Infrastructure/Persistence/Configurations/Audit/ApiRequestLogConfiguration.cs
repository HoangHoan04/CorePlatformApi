// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorePlatformApi.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// Cấu hình EF Core cho bảng <c>api_request_logs</c>.
/// </summary>
public sealed class ApiRequestLogConfiguration : IEntityTypeConfiguration<ApiRequestLog>
{
    public void Configure(EntityTypeBuilder<ApiRequestLog> builder)
    {
        builder.ToTable("api_request_logs", tb =>
        {
            tb.HasComment("Log request đi qua API Gateway (thường đẩy song song sang Loki/ELK; bảng này giữ ngắn hạn để tra cứu nhanh theo traceId)");
        });

        builder.HasKey(x => x.Id).HasName("api_request_logs_pkey");

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
        builder.Property(x => x.TraceId)
            .HasColumnName("trace_id")
            .HasColumnType("varchar(64)")
            .HasColumnOrder(2)
            .IsRequired()
            .HasComment("Trace id (trả về client qua header X-Trace-Id)");
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .HasColumnOrder(3)
            .HasComment("User");
        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(4)
            .HasComment("OAuth client");
        builder.Property(x => x.ApiKeyPrefix)
            .HasColumnName("api_key_prefix")
            .HasColumnType("varchar(16)")
            .HasColumnOrder(5)
            .HasComment("API key (tiền tố)");
        builder.Property(x => x.RouteCode)
            .HasColumnName("route_code")
            .HasColumnType("varchar(100)")
            .HasColumnOrder(6)
            .HasComment("Route khớp");
        builder.Property(x => x.ModuleCode)
            .HasColumnName("module_code")
            .HasColumnType("varchar(30)")
            .HasColumnOrder(7)
            .HasComment("Phân hệ");
        builder.Property(x => x.HttpMethod)
            .HasColumnName("http_method")
            .HasColumnType("varchar(10)")
            .HasColumnOrder(8)
            .IsRequired()
            .HasComment("Method");
        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasColumnType("varchar(2000)")
            .HasColumnOrder(9)
            .IsRequired()
            .HasComment("Path");
        builder.Property(x => x.QueryString)
            .HasColumnName("query_string")
            .HasColumnType("text")
            .HasColumnOrder(10)
            .HasComment("Query (đã che tham số nhạy cảm)");
        builder.Property(x => x.StatusCode)
            .HasColumnName("status_code")
            .HasColumnType("smallint")
            .HasColumnOrder(11)
            .IsRequired()
            .HasComment("HTTP status");
        builder.Property(x => x.ErrorCode)
            .HasColumnName("error_code")
            .HasColumnType("varchar(50)")
            .HasColumnOrder(12)
            .HasComment("Mã lỗi chuẩn (gateway.error_codes)");
        builder.Property(x => x.DurationMs)
            .HasColumnName("duration_ms")
            .HasColumnType("int")
            .HasColumnOrder(13)
            .IsRequired()
            .HasComment("Thời gian xử lý");
        builder.Property(x => x.UpstreamMs)
            .HasColumnName("upstream_ms")
            .HasColumnType("int")
            .HasColumnOrder(14)
            .HasComment("Thời gian upstream");
        builder.Property(x => x.RequestBytes)
            .HasColumnName("request_bytes")
            .HasColumnType("int")
            .HasColumnOrder(15)
            .HasComment("Kích thước request");
        builder.Property(x => x.ResponseBytes)
            .HasColumnName("response_bytes")
            .HasColumnType("int")
            .HasColumnOrder(16)
            .HasComment("Kích thước response");
        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasColumnType("inet")
            .HasColumnOrder(17)
            .HasComment("IP client");
        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("text")
            .HasColumnOrder(18)
            .HasComment("User-Agent");
        builder.Property(x => x.IsRateLimited)
            .HasColumnName("is_rate_limited")
            .HasColumnType("boolean")
            .HasColumnOrder(19)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Bị chặn rate limit");
        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(20)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm");
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(21)
            .IsRequired()
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever()
            .HasComment("Thời điểm tạo bản ghi");
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid")
            .HasColumnOrder(22)
            .HasComment("Người tạo (users.id); NULL nếu hệ thống tạo");
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(23)
            .HasComment("Thời điểm cập nhật gần nhất (trigger tự gán)");
        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid")
            .HasColumnOrder(24)
            .HasComment("Người cập nhật gần nhất (users.id)");
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasColumnOrder(25)
            .IsRequired()
            .HasDefaultValueSql("false")
            .ValueGeneratedNever()
            .HasComment("Cờ xóa mềm (true = đã xóa)");
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz")
            .HasColumnOrder(26)
            .HasComment("Thời điểm xóa mềm");
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid")
            .HasColumnOrder(27)
            .HasComment("Người xóa mềm (users.id)");

        builder.HasIndex(x => new { x.Id, x.OccurredAt }, "api_request_logs_id_occurred_at_key").IsUnique();
        builder.HasIndex(x => x.TraceId, "ix_api_logs_trace");
        builder.HasIndex(x => new { x.TenantId, x.OccurredAt }, "ix_api_logs_tenant")
            .IsDescending(false, true);
        builder.HasIndex(x => x.OccurredAt, "ix_api_logs_errors")
            .IsDescending(true)
            .HasFilter("status_code >= 500");
    }
}
