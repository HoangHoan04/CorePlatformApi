// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Push token của thiết bị (FCM registration token / APNs device token)
/// </summary>
/// <remarks>Bảng <c>notify.device_tokens</c>.</remarks>
public class DeviceToken
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Thiết bị
    /// </summary>
    /// <remarks>FK → <c>iam.user_devices(id)</c></remarks>
    public Guid? DeviceId { get; set; }

    /// <summary>
    /// App (HRM_MOBILE, WMS_MOBILE)
    /// </summary>
    public string AppCode { get; set; } = string.Empty;

    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    /// <remarks>Ràng buộc: <c>provider IN ('FCM','APNS','HMS','WEB_PUSH')</c></remarks>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Token push
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Ngôn ngữ app trên thiết bị
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// Còn hiệu lực (FCM báo UNREGISTERED → false)
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Lần gửi thành công gần nhất
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// Đăng ký lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Cập nhật token lúc
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Thiết bị đã đăng nhập của user (giới hạn thiết bị, thu hồi theo thiết bị) (qua <c>device_id</c>).
    /// </summary>
    public UserDevice? Device { get; set; }

    #endregion
}
