// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Mã khôi phục 2FA dùng một lần
/// </summary>
/// <remarks>Bảng <c>iam.user_recovery_codes</c>.</remarks>
public class UserRecoveryCode
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
    /// Hash mã khôi phục
    /// </summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm đã dùng (NULL = chưa dùng)
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>
    /// Thời điểm tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
