// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Audit;

/// <summary>
/// Log request đi qua API Gateway (thường đẩy song song sang Loki/ELK; bảng này giữ ngắn hạn để tra cứu nhanh theo traceId)
/// </summary>
/// <remarks>Bảng <c>audit.api_request_logs</c>; partition RANGE theo <c>occurred_at</c> (tạo bằng SQL thô, không qua EF migration).</remarks>
public class ApiRequestLog : IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Trace id (trả về client qua header X-Trace-Id)
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Tenant
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// User
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// OAuth client
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// API key (tiền tố)
    /// </summary>
    public string? ApiKeyPrefix { get; set; }

    /// <summary>
    /// Route khớp
    /// </summary>
    public string? RouteCode { get; set; }

    /// <summary>
    /// Phân hệ
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Method
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Path
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Query (đã che tham số nhạy cảm)
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// HTTP status
    /// </summary>
    public short StatusCode { get; set; }

    /// <summary>
    /// Mã lỗi chuẩn (gateway.error_codes)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Thời gian xử lý
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Thời gian upstream
    /// </summary>
    public int? UpstreamMs { get; set; }

    /// <summary>
    /// Kích thước request
    /// </summary>
    public int? RequestBytes { get; set; }

    /// <summary>
    /// Kích thước response
    /// </summary>
    public int? ResponseBytes { get; set; }

    /// <summary>
    /// IP client
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Bị chặn rate limit
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsRateLimited { get; set; } = false;

    /// <summary>
    /// Thời điểm
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
