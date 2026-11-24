// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// Transactional Outbox: event được ghi cùng transaction nghiệp vụ, worker đọc và publish lên message broker
/// </summary>
/// <remarks>Bảng <c>outbox_messages</c> (phân hệ integration).</remarks>
public class OutboxMessage : BaseEntity
{
    /// <summary>
    /// Mã event
    /// </summary>
    public string EventTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Loại aggregate (Order, Employee)
    /// </summary>
    public string? AggregateType { get; set; }

    /// <summary>
    /// Id aggregate
    /// </summary>
    public Guid? AggregateId { get; set; }

    /// <summary>
    /// Nội dung event
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Header (user, correlation...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Headers { get; set; } = "{}";

    /// <summary>
    /// Trace id
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Trạng thái publish
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','PUBLISHED','FAILED','DEAD')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// Số lần thử
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short Attempts { get; set; } = 0;

    /// <summary>
    /// Lần thử tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Lỗi gần nhất
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Thời điểm sự kiện xảy ra
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Publish thành công lúc
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }
}
