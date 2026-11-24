// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Phiên đăng nhập SSO (sid). Một phiên trên một thiết bị/client; thu hồi phiên → thu hồi toàn bộ refresh token
/// </summary>
/// <remarks>Bảng <c>user_sessions</c> (phân hệ iam).</remarks>
public class UserSession : BaseEntity
{
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
    /// Ứng dụng khởi tạo phiên (iam.oauth_clients.client_id)
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Phương thức xác thực
    /// </summary>
    /// <remarks>Ràng buộc: <c>auth_method IN ('PASSWORD','PASSWORD_MFA','EXTERNAL','OTP','IMPERSONATION','API')</c>; Mặc định DB: <c>'PASSWORD'</c></remarks>
    public string AuthMethod { get; set; } = "PASSWORD";

    /// <summary>
    /// Ngữ cảnh công ty đang chọn
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Ngữ cảnh chi nhánh đang chọn
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// IP khởi tạo
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Admin đang đăng nhập thay (hỗ trợ khách hàng)
    /// </summary>
    public Guid? ImpersonatorId { get; set; }

    /// <summary>
    /// Bắt đầu
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Hoạt động cuối (tính idle timeout)
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset LastActivityAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Hết hạn tuyệt đối
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Thu hồi lúc
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Lý do thu hồi
    /// </summary>
    /// <remarks>Ràng buộc: <c>revoked_reason IN ('LOGOUT','ADMIN_REVOKE','PASSWORD_CHANGED','ACCOUNT_LOCKED','DEVICE_LIMIT','TOKEN_REUSE','EXPIRED','ROLE_CHANGED')</c></remarks>
    public string? RevokedReason { get; set; }

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
    /// Điều hướng tới Thiết bị đã đăng nhập của user (giới hạn thiết bị, thu hồi theo thiết bị) (qua <c>device_id</c>).
    /// </summary>
    public UserDevice? Device { get; set; }

    /// <summary>
    /// Danh sách RefreshToken con (qua <c>iam.refresh_tokens.session_id</c>).
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    #endregion
}
