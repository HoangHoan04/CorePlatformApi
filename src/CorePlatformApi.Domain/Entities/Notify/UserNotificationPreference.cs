// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Tùy chọn nhận thông báo của user theo sự kiện × kênh, giờ yên lặng
/// </summary>
/// <remarks>Bảng <c>user_preferences</c> (phân hệ notify).</remarks>
public class UserNotificationPreference : BaseEntity
{
    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Sự kiện (NULL = áp dụng mọi sự kiện)
    /// </summary>
    /// <remarks>FK → <c>notify.event_types(id)</c></remarks>
    public Guid? EventTypeId { get; set; }

    /// <summary>
    /// Kênh
    /// </summary>
    /// <remarks>Ràng buộc: <c>channel IN ('IN_APP','PUSH','EMAIL','SMS','ZALO')</c></remarks>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Bật/tắt
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Bắt đầu giờ yên lặng (không push)
    /// </summary>
    public TimeOnly? QuietStart { get; set; }

    /// <summary>
    /// Kết thúc giờ yên lặng
    /// </summary>
    public TimeOnly? QuietEnd { get; set; }

    /// <summary>
    /// Gửi ngay hay gom tóm tắt
    /// </summary>
    /// <remarks>Ràng buộc: <c>digest_mode IN ('INSTANT','HOURLY','DAILY')</c>; Mặc định DB: <c>'INSTANT'</c></remarks>
    public string DigestMode { get; set; } = "INSTANT";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Loại sự kiện thông báo do các phân hệ khai báo (hrm (qua <c>event_type_id</c>).
    /// </summary>
    public NotificationEventType? EventType { get; set; }

    #endregion
}
