// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Endpoint webhook OUTBOUND: ERP bắn sự kiện ra hệ thống bên thứ 3
/// </summary>
/// <remarks>Bảng <c>webhook_endpoints</c> (phân hệ integration); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class WebhookEndpoint : BaseEntity
{
    /// <summary>
    /// Tên endpoint
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL nhận
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Phương thức
    /// </summary>
    /// <remarks>Ràng buộc: <c>http_method IN ('POST','PUT','PATCH')</c>; Mặc định DB: <c>'POST'</c></remarks>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// Event đăng ký (integration.event_types.code), hỗ trợ wildcard 'crm.order.*'
    /// </summary>
    public List<string> EventTypes { get; set; } = [];

    /// <summary>
    /// Secret ký HMAC-SHA256 (header X-Signature)
    /// </summary>
    public string? SecretEncrypted { get; set; }

    /// <summary>
    /// Header bổ sung (Authorization...) — giá trị nhạy cảm nên mã hóa
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string CustomHeaders { get; set; } = "{}";

    /// <summary>
    /// Content-Type
    /// </summary>
    /// <remarks>Mặc định DB: <c>'application/json'</c></remarks>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Timeout mỗi lần gọi
    /// </summary>
    /// <remarks>Mặc định DB: <c>10</c></remarks>
    public short TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Số lần retry tối đa
    /// </summary>
    /// <remarks>Mặc định DB: <c>8</c></remarks>
    public short MaxRetries { get; set; } = 8;

    /// <summary>
    /// Chiến lược retry
    /// </summary>
    /// <remarks>Ràng buộc: <c>retry_strategy IN ('FIXED','EXPONENTIAL')</c>; Mặc định DB: <c>'EXPONENTIAL'</c></remarks>
    public string RetryStrategy { get; set; } = "EXPONENTIAL";

    /// <summary>
    /// Khoảng chờ cơ sở giữa các lần retry
    /// </summary>
    /// <remarks>Mặc định DB: <c>30</c></remarks>
    public int RetryBaseSeconds { get; set; } = 30;

    /// <summary>
    /// Chỉ gửi khi payload thỏa điều kiện
    /// </summary>
    public string? FilterCondition { get; set; }

    /// <summary>
    /// Số lần lỗi liên tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int ConsecutiveFailures { get; set; } = 0;

    /// <summary>
    /// Tự tắt sau N lần lỗi liên tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>50</c></remarks>
    public int AutoDisableAfter { get; set; } = 50;

    /// <summary>
    /// Lần gửi thành công gần nhất
    /// </summary>
    public DateTimeOffset? LastSuccessAt { get; set; }

    /// <summary>
    /// Lần lỗi gần nhất
    /// </summary>
    public DateTimeOffset? LastFailureAt { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','PAUSED','DISABLED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Danh sách WebhookDelivery con (qua <c>integration.webhook_deliveries.endpoint_id</c>).
    /// </summary>
    public ICollection<WebhookDelivery> WebhookDeliveries { get; set; } = [];

    #endregion
}
