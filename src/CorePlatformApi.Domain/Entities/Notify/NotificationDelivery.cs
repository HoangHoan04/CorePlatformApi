// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Hàng đợi/nhật ký gửi theo từng kênh (outbox): email, push, SMS... có retry
/// </summary>
/// <remarks>Bảng <c>notify.deliveries</c>.</remarks>
public class NotificationDelivery : IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Thông báo gốc
    /// </summary>
    /// <remarks>FK → <c>notify.notifications(id)</c></remarks>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// Tenant
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Người nhận (NULL nếu gửi tới email ngoài hệ thống)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Kênh
    /// </summary>
    /// <remarks>Ràng buộc: <c>channel IN ('PUSH','EMAIL','SMS','ZALO','WEBHOOK')</c></remarks>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ đích: email, SĐT, device token id
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Template đã dùng
    /// </summary>
    /// <remarks>FK → <c>notify.templates(id)</c></remarks>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Cấu hình gửi (smtp_configs / push_configs)
    /// </summary>
    public Guid? ConfigId { get; set; }

    /// <summary>
    /// Tiêu đề đã render
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Nội dung đã render
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Trạng thái gửi
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','SENDING','SENT','DELIVERED','OPENED','FAILED','CANCELLED','SKIPPED')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// Số lần đã thử
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short Attempts { get; set; } = 0;

    /// <summary>
    /// Số lần thử tối đa
    /// </summary>
    /// <remarks>Mặc định DB: <c>5</c></remarks>
    public short MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Lần thử kế tiếp (exponential backoff)
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Message id phía provider (đối soát)
    /// </summary>
    public string? ProviderMessageId { get; set; }

    /// <summary>
    /// Lỗi gần nhất
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gửi thành công lúc
    /// </summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>
    /// Thời điểm tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Thông báo gốc phát sinh từ một sự kiện nghiệp vụ (một thông báo → nhiều người nhận, nhiều kênh) (qua <c>notification_id</c>).
    /// </summary>
    public Notification Notification { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Mẫu nội dung theo sự kiện × kênh × ngôn ngữ, dùng biến {{employee_name}} (qua <c>template_id</c>).
    /// </summary>
    public NotificationTemplate? Template { get; set; }

    /// <summary>
    /// Danh sách NotificationDeliveryAttachment con (qua <c>notify.delivery_attachments.delivery_id</c>).
    /// </summary>
    public ICollection<NotificationDeliveryAttachment> NotificationDeliveryAttachments { get; set; } = [];

    #endregion
}
