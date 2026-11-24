// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Lịch sử mật khẩu để chặn dùng lại N mật khẩu gần nhất
/// </summary>
/// <remarks>Bảng <c>password_histories</c> (phân hệ iam).</remarks>
public class PasswordHistory : BaseEntity
{
    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hash mật khẩu cũ
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
