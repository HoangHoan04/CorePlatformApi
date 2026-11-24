namespace CorePlatformApi.Application.Common.Interfaces;

/// <summary>
/// Thông tin người dùng của request hiện tại (lấy từ JWT).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// iam.users.id (claim <c>sub</c>).
    /// </summary>
    Guid? UserId { get; }
    /// <summary>
    /// org.tenants.id (claim <c>tenant_id</c>).
    /// </summary>
    Guid? TenantId { get; }
    /// <summary>
    /// iam.user_sessions.id (claim <c>sid</c>).
    /// </summary>
    Guid? SessionId { get; }
    bool IsPlatformAdmin { get; }
    string? Username { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    /// <summary>
    /// Ngôn ngữ giao diện của request (header Accept-Language, VD vi-VN) — dùng để dịch thông báo / Excel.
    /// </summary>
    string? Language { get; }
}
