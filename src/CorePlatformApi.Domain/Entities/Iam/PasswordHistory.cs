// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Lịch sử mật khẩu để chặn dùng lại N mật khẩu gần nhất
/// </summary>
/// <remarks>Bảng <c>iam.password_histories</c>.</remarks>
public class PasswordHistory
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
    /// Hash mật khẩu cũ
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm đặt mật khẩu này
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
