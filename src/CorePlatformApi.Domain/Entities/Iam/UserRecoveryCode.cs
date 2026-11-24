// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Mã khôi phục 2FA dùng một lần
/// </summary>
/// <remarks>Bảng <c>user_recovery_codes</c> (phân hệ iam).</remarks>
public class UserRecoveryCode : BaseEntity
{
    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hash mã khôi phục
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm đã dùng (NULL = chưa dùng)
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
