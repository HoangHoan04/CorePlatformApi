// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Audit;

/// <summary>
/// Sự kiện bảo mật quan trọng: đổi/reset mật khẩu, khóa/mở khóa, thay đổi vai trò/quyền, cấp API key, bật/tắt 2FA
/// </summary>
/// <remarks>Bảng <c>security_events</c> (phân hệ audit).</remarks>
public class SecurityEvent : BaseEntity
{
    /// <summary>
    /// Loại (PASSWORD_CHANGED, PASSWORD_RESET, USER_LOCKED, USER_UNLOCKED, ROLE_ASSIGNED, ROLE_REVOKED, ROLE_PERMISSION_CHANGED, MFA_ENABLED, API_KEY_CREATED, SESSION_REVOKED...)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Mức độ
    /// </summary>
    /// <remarks>Ràng buộc: <c>severity IN ('INFO','WARNING','CRITICAL')</c>; Mặc định DB: <c>'INFO'</c></remarks>
    public string Severity { get; set; } = "INFO";

    /// <summary>
    /// Người thực hiện (NULL = hệ thống)
    /// </summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>
    /// User bị tác động
    /// </summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>
    /// Loại đối tượng bị tác động (ROLE, API_KEY, OAUTH_CLIENT)
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// Id đối tượng
    /// </summary>
    public Guid? TargetId { get; set; }

    /// <summary>
    /// Mô tả dễ đọc
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Chi tiết (quyền thêm/bớt, vai trò cũ/mới)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Trace id
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Thời điểm
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
