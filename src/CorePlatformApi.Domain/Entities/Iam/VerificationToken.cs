// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Token một lần: quên mật khẩu, xác thực email/SĐT, lời mời kích hoạt, OTP đăng nhập
/// </summary>
/// <remarks>Bảng <c>verification_tokens</c> (phân hệ iam).</remarks>
public class VerificationToken : BaseEntity
{
    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Mục đích
    /// </summary>
    /// <remarks>Ràng buộc: <c>purpose IN ('RESET_PASSWORD','VERIFY_EMAIL','VERIFY_PHONE','INVITATION','LOGIN_OTP','CHANGE_EMAIL')</c></remarks>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Hash của token/OTP (không lưu bản rõ)
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Email/SĐT nhận token (khi đổi email)
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Số lần nhập sai OTP
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int Attempts { get; set; } = 0;

    /// <summary>
    /// Hết hạn
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Đã dùng lúc
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>
    /// IP yêu cầu
    /// </summary>
    public IPAddress? CreatedIp { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
