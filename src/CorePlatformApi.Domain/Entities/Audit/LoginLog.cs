// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Audit;

/// <summary>
/// Lịch sử đăng nhập/đăng xuất/refresh token (thành công &amp; thất bại) kèm IP, thiết bị
/// </summary>
/// <remarks>Bảng <c>audit.login_logs</c>; partition RANGE theo <c>occurred_at</c> (tạo bằng SQL thô, không qua EF migration).</remarks>
public class LoginLog : IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// User (NULL nếu username không tồn tại)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Username/email đã nhập
    /// </summary>
    public string? UsernameAttempted { get; set; }

    /// <summary>
    /// Sự kiện
    /// </summary>
    /// <remarks>Ràng buộc: <c>event_type IN ('LOGIN_SUCCESS','LOGIN_FAILED','LOGOUT','TOKEN_REFRESH','TOKEN_REUSE_DETECTED','MFA_SUCCESS','MFA_FAILED','ACCOUNT_LOCKED','SSO_LOGIN','IMPERSONATE')</c></remarks>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Lý do thất bại (INVALID_PASSWORD, USER_NOT_FOUND, ACCOUNT_LOCKED, IP_NOT_ALLOWED, DEVICE_LIMIT)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Phương thức (PASSWORD, GOOGLE, OTP)
    /// </summary>
    public string? AuthMethod { get; set; }

    /// <summary>
    /// Ứng dụng
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Phiên tạo ra
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Thiết bị
    /// </summary>
    public Guid? DeviceId { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Quốc gia (GeoIP)
    /// </summary>
    public string? GeoCountry { get; set; }

    /// <summary>
    /// Thành phố (GeoIP)
    /// </summary>
    public string? GeoCity { get; set; }

    /// <summary>
    /// Đánh dấu bất thường (IP lạ, quốc gia lạ)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsSuspicious { get; set; } = false;

    /// <summary>
    /// Thời điểm
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
