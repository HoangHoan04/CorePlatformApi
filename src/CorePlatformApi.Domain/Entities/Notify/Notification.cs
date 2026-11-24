// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Thông báo gốc phát sinh từ một sự kiện nghiệp vụ (một thông báo → nhiều người nhận, nhiều kênh)
/// </summary>
/// <remarks>Bảng <c>notifications</c> (phân hệ notify).</remarks>
public class Notification : BaseEntity
{
    /// <summary>
    /// Loại sự kiện (NULL = thông báo thủ công/broadcast)
    /// </summary>
    /// <remarks>FK → <c>notify.event_types(id)</c></remarks>
    public Guid? EventTypeId { get; set; }

    /// <summary>
    /// Phân hệ phát sinh
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Đối tượng liên quan
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Id đối tượng liên quan
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Ưu tiên
    /// </summary>
    /// <remarks>Ràng buộc: <c>priority IN ('LOW','NORMAL','HIGH','URGENT')</c>; Mặc định DB: <c>'NORMAL'</c></remarks>
    public string Priority { get; set; } = "NORMAL";

    /// <summary>
    /// Dữ liệu biến để render template
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Payload { get; set; } = "{}";

    /// <summary>
    /// Kênh sẽ gửi
    /// </summary>
    public List<string> Channels { get; set; } = [];

    /// <summary>
    /// Hẹn giờ gửi (NULL = gửi ngay)
    /// </summary>
    public DateTimeOffset? ScheduledAt { get; set; }

    /// <summary>
    /// Chống gửi trùng khi event bị phát lại
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Người gửi (thông báo thủ công)
    /// </summary>
    public Guid? SenderUserId { get; set; }

    /// <summary>
    /// Trace id liên kết request gốc
    /// </summary>
    public string? TraceId { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Loại sự kiện thông báo do các phân hệ khai báo (hrm (qua <c>event_type_id</c>).
    /// </summary>
    public NotificationEventType? EventType { get; set; }

    /// <summary>
    /// Danh sách UserNotification con (qua <c>notify.user_notifications.notification_id</c>).
    /// </summary>
    public ICollection<UserNotification> UserNotifications { get; set; } = [];

    /// <summary>
    /// Danh sách NotificationDelivery con (qua <c>notify.deliveries.notification_id</c>).
    /// </summary>
    public ICollection<NotificationDelivery> NotificationDeliveries { get; set; } = [];

    #endregion
}
