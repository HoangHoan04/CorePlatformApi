// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Entities.Notify;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Thiết bị đã đăng nhập của user (giới hạn thiết bị, thu hồi theo thiết bị)
/// </summary>
/// <remarks>Bảng <c>iam.user_devices</c>.</remarks>
public class UserDevice
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
    /// Định danh thiết bị do client sinh và lưu (fingerprint/installation id)
    /// </summary>
    public string DeviceKey { get; set; } = string.Empty;

    /// <summary>
    /// Tên thiết bị (iPhone 15 của Hoàng, Chrome - Windows)
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// Nền tảng
    /// </summary>
    /// <remarks>Ràng buộc: <c>platform IN ('WEB','ANDROID','IOS','DESKTOP','OTHER')</c></remarks>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Phiên bản hệ điều hành
    /// </summary>
    public string? OsVersion { get; set; }

    /// <summary>
    /// Phiên bản app
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// User-Agent gần nhất
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP gần nhất
    /// </summary>
    public IPAddress? LastIp { get; set; }

    /// <summary>
    /// Thiết bị tin cậy (bỏ qua 2FA)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsTrusted { get; set; } = false;

    /// <summary>
    /// Lần đầu thấy
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Lần cuối hoạt động
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','REVOKED','BLOCKED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Thời điểm thu hồi
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Người thu hồi
    /// </summary>
    public Guid? RevokedBy { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Danh sách DeviceToken con (qua <c>notify.device_tokens.device_id</c>).
    /// </summary>
    public ICollection<DeviceToken> DeviceTokens { get; set; } = [];

    #endregion
}
