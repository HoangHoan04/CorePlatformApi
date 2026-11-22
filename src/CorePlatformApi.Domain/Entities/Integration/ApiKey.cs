// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Integration;

/// <summary>
/// API Key cấp cho đối tác/ứng dụng ngoài gọi vào ERP, có scope, IP whitelist, hạn dùng
/// </summary>
/// <remarks>Bảng <c>integration.api_keys</c>.</remarks>
public class ApiKey : AuditableEntity, IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gắn với client OAuth (nếu có)
    /// </summary>
    /// <remarks>FK → <c>iam.oauth_clients(id)</c></remarks>
    public Guid? OauthClientId { get; set; }

    /// <summary>
    /// Tên key (Kết nối website bán hàng)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tiền tố hiển thị để nhận diện (berp_live_ab12)
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 của API key (bản rõ chỉ hiển thị 1 lần khi tạo)
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Scope được phép (iam.api_scopes.code)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public List<string> Scopes { get; set; } = [];

    /// <summary>
    /// Người chịu trách nhiệm
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid? OwnerUserId { get; set; }

    /// <summary>
    /// Thực thi dưới danh nghĩa user (để áp RBAC/data scope)
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid? ActAsUserId { get; set; }

    /// <summary>
    /// Môi trường
    /// </summary>
    /// <remarks>Ràng buộc: <c>environment IN ('LIVE','TEST')</c>; Mặc định DB: <c>'LIVE'</c></remarks>
    public string Environment { get; set; } = "LIVE";

    /// <summary>
    /// IP được phép
    /// </summary>
    public List<IPNetwork>? IpWhitelist { get; set; }

    /// <summary>
    /// Origin được phép (gọi từ trình duyệt)
    /// </summary>
    public List<string>? AllowedOrigins { get; set; }

    /// <summary>
    /// Chính sách rate limit
    /// </summary>
    public string? RateLimitPolicyCode { get; set; }

    /// <summary>
    /// Hết hạn
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Lần dùng gần nhất
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// IP dùng gần nhất
    /// </summary>
    public IPAddress? LastUsedIp { get; set; }

    /// <summary>
    /// Tổng số lần gọi
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public long UsageCount { get; set; } = 0;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','REVOKED','EXPIRED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Thu hồi lúc
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Người thu hồi
    /// </summary>
    public Guid? RevokedBy { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Ứng dụng client (OAuth2/OIDC): Core Admin, HRM Web, HRM Mobile, WMS Mobile, service nội bộ, ứng dụng đối tác (qua <c>oauth_client_id</c>).
    /// </summary>
    public OauthClient? OauthClient { get; set; }

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>owner_user_id</c>).
    /// </summary>
    public User? OwnerUser { get; set; }

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>act_as_user_id</c>).
    /// </summary>
    public User? ActAsUser { get; set; }

    #endregion
}
