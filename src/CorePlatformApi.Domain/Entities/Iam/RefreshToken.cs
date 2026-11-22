// Nguồn schema: docs/base_erp_core.sql
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Refresh token có rotation: mỗi lần refresh sinh token mới cùng family; dùng lại token cũ → thu hồi cả family (phát hiện đánh cắp)
/// </summary>
/// <remarks>Bảng <c>iam.refresh_tokens</c>.</remarks>
public class RefreshToken
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Phiên
    /// </summary>
    /// <remarks>FK → <c>iam.user_sessions(id)</c></remarks>
    public Guid SessionId { get; set; }

    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Client được cấp
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// SHA-256 của refresh token (không lưu bản rõ)
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Họ token: chung cho chuỗi rotation từ một lần đăng nhập
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Token trước đó trong chuỗi rotation
    /// </summary>
    /// <remarks>FK → <c>iam.refresh_tokens(id)</c></remarks>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Token mới thay thế (khi đã rotate)
    /// </summary>
    public Guid? ReplacedById { get; set; }

    /// <summary>
    /// Scope được cấp
    /// </summary>
    public List<string>? Scopes { get; set; }

    /// <summary>
    /// Thời điểm cấp
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Hết hạn
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Đã dùng để rotate lúc
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>
    /// Thu hồi lúc
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Lý do thu hồi
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// IP yêu cầu cấp
    /// </summary>
    public IPAddress? CreatedIp { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Phiên đăng nhập SSO (sid) (qua <c>session_id</c>).
    /// </summary>
    public UserSession Session { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Refresh token có rotation: mỗi lần refresh sinh token mới cùng family; dùng lại token cũ → thu hồi cả family (phát hiện đánh cắp) (qua <c>parent_id</c>).
    /// </summary>
    public RefreshToken? Parent { get; set; }

    /// <summary>
    /// Danh sách RefreshToken con (qua <c>iam.refresh_tokens.parent_id</c>).
    /// </summary>
    public ICollection<RefreshToken> Children { get; set; } = [];

    #endregion
}
