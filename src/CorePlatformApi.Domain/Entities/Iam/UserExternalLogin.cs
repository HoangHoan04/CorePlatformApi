// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Liên kết đăng nhập ngoài (Google, Microsoft, Zalo, Apple) với tài khoản nội bộ
/// </summary>
/// <remarks>Bảng <c>user_external_logins</c> (phân hệ iam).</remarks>
public class UserExternalLogin : BaseEntity
{
    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Nhà cung cấp định danh
    /// </summary>
    /// <remarks>Ràng buộc: <c>provider IN ('GOOGLE','MICROSOFT','APPLE','ZALO','LDAP','SAML','OIDC')</c></remarks>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Id user phía provider (sub)
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Email phía provider
    /// </summary>
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// Thời điểm liên kết
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Lần dùng gần nhất
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
