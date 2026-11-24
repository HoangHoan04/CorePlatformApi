// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Inbox/Idempotency: consumer ghi nhận message đã xử lý để không xử lý trùng
/// </summary>
/// <remarks>Bảng <c>inbox_messages</c> (phân hệ integration).</remarks>
public class InboxMessage : BaseEntity
{
    /// <summary>
    /// Service tiêu thụ
    /// </summary>
    public string ConsumerService { get; set; } = string.Empty;

    /// <summary>
    /// Message id
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Mã event
    /// </summary>
    public string? EventTypeCode { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PROCESSING','PROCESSED','FAILED')</c>; Mặc định DB: <c>'PROCESSED'</c></remarks>
    public string Status { get; set; } = "PROCESSED";

    /// <summary>
    /// Lỗi
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Nhận lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Xử lý xong lúc
    /// </summary>
    public DateTimeOffset? ProcessedAt { get; set; }
}
