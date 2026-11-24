// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Nhật ký từng lần gửi webhook outbound (payload, response, retry)
/// </summary>
/// <remarks>Bảng <c>webhook_deliveries</c> (phân hệ integration).</remarks>
public class WebhookDelivery : BaseEntity
{
    /// <summary>
    /// Endpoint
    /// </summary>
    /// <remarks>FK → <c>integration.webhook_endpoints(id)</c></remarks>
    public Guid EndpointId { get; set; }

    /// <summary>
    /// Event gốc (outbox_messages.id)
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// Mã event
    /// </summary>
    public string EventTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// URL thực tế
    /// </summary>
    public string RequestUrl { get; set; } = string.Empty;

    /// <summary>
    /// Header đã gửi (đã che secret)
    /// </summary>
    public string? RequestHeaders { get; set; }

    /// <summary>
    /// Payload đã gửi
    /// </summary>
    public string RequestBody { get; set; } = string.Empty;

    /// <summary>
    /// Lần thử thứ
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short AttemptNo { get; set; } = 1;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','SUCCESS','FAILED','RETRYING','DEAD')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// HTTP status trả về
    /// </summary>
    public short? ResponseStatus { get; set; }

    /// <summary>
    /// Body trả về (cắt tối đa vài KB)
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Thời gian phản hồi
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Lỗi mạng/timeout
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Lần retry tiếp
    /// </summary>
    public DateTimeOffset? NextRetryAt { get; set; }

    /// <summary>
    /// Hoàn tất lúc
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Endpoint webhook OUTBOUND: ERP bắn sự kiện ra hệ thống bên thứ 3 (qua <c>endpoint_id</c>).
    /// </summary>
    public WebhookEndpoint Endpoint { get; set; } = null!;

    #endregion
}
