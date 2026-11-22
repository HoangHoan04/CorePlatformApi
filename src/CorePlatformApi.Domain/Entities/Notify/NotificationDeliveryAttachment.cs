// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Storage;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// File đính kèm email
/// </summary>
/// <remarks>Bảng <c>notify.delivery_attachments</c>.</remarks>
public class NotificationDeliveryAttachment
{
    /// <summary>
    /// Lượt gửi
    /// </summary>
    /// <remarks>FK → <c>notify.deliveries(id)</c></remarks>
    public Guid DeliveryId { get; set; }

    /// <summary>
    /// File đính kèm
    /// </summary>
    /// <remarks>FK → <c>storage.files(id)</c></remarks>
    public Guid FileId { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Hàng đợi/nhật ký gửi theo từng kênh (outbox): email, push, SMS (qua <c>delivery_id</c>).
    /// </summary>
    public NotificationDelivery Delivery { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>file_id</c>).
    /// </summary>
    public StorageFile File { get; set; } = null!;

    #endregion
}
