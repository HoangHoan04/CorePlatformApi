// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Hộp thư thông báo in-app của từng user (thanh chuông). Đã render sẵn nội dung theo ngôn ngữ user
/// </summary>
/// <remarks>Bảng <c>notify.user_notifications</c>.</remarks>
public class UserNotification : IMayHaveTenant
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
    /// Tenant (denormalize)
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Người nhận
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Phân hệ (lọc theo tab)
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Tiêu đề đã render
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung đã render
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Icon/ảnh đại diện người gửi
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Link khi bấm
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Nhóm hiển thị
    /// </summary>
    /// <remarks>Ràng buộc: <c>category IN ('INFO','TASK','APPROVAL','WARNING','SYSTEM')</c>; Mặc định DB: <c>'INFO'</c></remarks>
    public string Category { get; set; } = "INFO";

    /// <summary>
    /// Đã đọc
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Đọc lúc
    /// </summary>
    public DateTimeOffset? ReadAt { get; set; }

    /// <summary>
    /// Ghim
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// Đã lưu trữ/ẩn
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Thời điểm nhận
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Thông báo gốc phát sinh từ một sự kiện nghiệp vụ (một thông báo → nhiều người nhận, nhiều kênh) (qua <c>notification_id</c>).
    /// </summary>
    public Notification Notification { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
