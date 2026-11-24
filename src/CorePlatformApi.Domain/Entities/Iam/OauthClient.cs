// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Ứng dụng client (OAuth2/OIDC): Core Admin, HRM Web, HRM Mobile, WMS Mobile, service nội bộ, ứng dụng đối tác
/// </summary>
/// <remarks>Bảng <c>oauth_clients</c> (phân hệ iam); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class OauthClient : BaseEntity
{
    /// <summary>
    /// client_id công khai
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Hash client_secret (NULL với public client như SPA/Mobile dùng PKCE)
    /// </summary>
    public string? ClientSecretHash { get; set; }

    /// <summary>
    /// Tên ứng dụng
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Phân hệ gắn với client (HRM Mobile → HRM)
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Public (SPA, mobile) hay Confidential (backend)
    /// </summary>
    /// <remarks>Ràng buộc: <c>client_type IN ('PUBLIC','CONFIDENTIAL')</c></remarks>
    public string ClientType { get; set; } = string.Empty;

    /// <summary>
    /// Loại ứng dụng
    /// </summary>
    /// <remarks>Ràng buộc: <c>app_type IN ('WEB','SPA','MOBILE','SERVICE','PARTNER')</c></remarks>
    public string AppType { get; set; } = string.Empty;

    /// <summary>
    /// Grant cho phép: authorization_code, refresh_token, client_credentials, password
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{authorization_code,refresh_token}'</c></remarks>
    public List<string> GrantTypes { get; set; } = ["authorization_code", "refresh_token"];

    /// <summary>
    /// Danh sách redirect URI hợp lệ
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public List<string> RedirectUris { get; set; } = [];

    /// <summary>
    /// URI chuyển hướng sau đăng xuất
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public List<string> PostLogoutRedirectUris { get; set; } = [];

    /// <summary>
    /// Origin CORS cho client
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public List<string> AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Scope cho phép (iam.api_scopes.code)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{openid,profile,offline_access}'</c></remarks>
    public List<string> AllowedScopes { get; set; } = ["openid", "profile", "offline_access"];

    /// <summary>
    /// Bắt buộc PKCE
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool RequirePkce { get; set; } = true;

    /// <summary>
    /// Hiển thị màn hình đồng ý (ứng dụng bên thứ 3)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool RequireConsent { get; set; } = false;

    /// <summary>
    /// Ghi đè thời hạn access token
    /// </summary>
    public int? AccessTokenMinutes { get; set; }

    /// <summary>
    /// Ghi đè thời hạn refresh token
    /// </summary>
    public int? RefreshTokenDays { get; set; }

    /// <summary>
    /// Bật Refresh Token Rotation
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool RotateRefreshToken { get; set; } = true;

    /// <summary>
    /// IP được phép dùng client_credentials
    /// </summary>
    public List<IPNetwork>? IpWhitelist { get; set; }

    /// <summary>
    /// Chính sách rate limit áp dụng (gateway.rate_limit_policies.code)
    /// </summary>
    public string? RateLimitPolicyCode { get; set; }

    /// <summary>
    /// Logo hiển thị trên trang đồng ý
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Hạn của client_secret
    /// </summary>
    public DateTimeOffset? SecretExpiresAt { get; set; }

    /// <summary>
    /// Lần dùng gần nhất
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','DISABLED','REVOKED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    #endregion
}
