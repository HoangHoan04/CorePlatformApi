// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Route định tuyến: /auth/*, /hrm/*, /wms/*, /crm/*, /ai/* → cluster nội bộ
/// </summary>
/// <remarks>Bảng <c>gateway.routes</c>.</remarks>
public class GatewayRoute : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã route
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Cluster đích
    /// </summary>
    /// <remarks>FK → <c>gateway.clusters(id)</c></remarks>
    public Guid ClusterId { get; set; }

    /// <summary>
    /// Mẫu path (/hrm/{**catch-all})
    /// </summary>
    public string PathPattern { get; set; } = string.Empty;

    /// <summary>
    /// Method khớp (NULL = tất cả)
    /// </summary>
    public List<string>? Methods { get; set; }

    /// <summary>
    /// Host khớp (NULL = tất cả)
    /// </summary>
    public List<string>? Hosts { get; set; }

    /// <summary>
    /// Biến đổi path (bỏ tiền tố /hrm → /api)
    /// </summary>
    public string? PathTransform { get; set; }

    /// <summary>
    /// Thứ tự khớp (nhỏ ưu tiên)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int OrderNo { get; set; } = 0;

    /// <summary>
    /// Cách xác thực tại gateway: verify JWT cục bộ hoặc introspect với IdP
    /// </summary>
    /// <remarks>Ràng buộc: <c>auth_mode IN ('ANONYMOUS','JWT','INTROSPECT','API_KEY','JWT_OR_API_KEY','MTLS')</c>; Mặc định DB: <c>'JWT'</c></remarks>
    public string AuthMode { get; set; } = "JWT";

    /// <summary>
    /// Scope bắt buộc
    /// </summary>
    public List<string>? RequiredScopes { get; set; }

    /// <summary>
    /// Quyền bắt buộc kiểm tra sớm tại gateway (tùy chọn)
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Rate limit
    /// </summary>
    /// <remarks>FK → <c>gateway.rate_limit_policies(id)</c></remarks>
    public Guid? RateLimitPolicyId { get; set; }

    /// <summary>
    /// CORS
    /// </summary>
    /// <remarks>FK → <c>gateway.cors_policies(id)</c></remarks>
    public Guid? CorsPolicyId { get; set; }

    /// <summary>
    /// Security header
    /// </summary>
    /// <remarks>FK → <c>gateway.security_header_policies(id)</c></remarks>
    public Guid? SecurityHeaderPolicyId { get; set; }

    /// <summary>
    /// Timeout riêng (ghi đè cluster)
    /// </summary>
    public int? TimeoutMs { get; set; }

    /// <summary>
    /// Giới hạn kích thước body (upload)
    /// </summary>
    public int? MaxRequestBodyMb { get; set; }

    /// <summary>
    /// Retry khi upstream lỗi (chỉ method idempotent)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short RetryCount { get; set; } = 0;

    /// <summary>
    /// Cache response tại gateway (danh mục MDM)
    /// </summary>
    public int? CacheSeconds { get; set; }

    /// <summary>
    /// Header thêm khi chuyển tiếp (X-User-Id, X-Tenant-Id, X-Permissions)
    /// </summary>
    public string? ForwardHeaders { get; set; }

    /// <summary>
    /// Bọc response/lỗi theo định dạng chuẩn
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool WrapResponse { get; set; } = true;

    /// <summary>
    /// Đang bật
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thông báo bảo trì (khi có giá trị gateway trả 503)
    /// </summary>
    public string? MaintenanceMessage { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Điều hướng tới Cụm upstream (service nội bộ) mà gateway định tuyến tới (qua <c>cluster_id</c>).
    /// </summary>
    public GatewayCluster Cluster { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chính sách rate limit/throttling theo IP, user, tenant, client, API key (qua <c>rate_limit_policy_id</c>).
    /// </summary>
    public RateLimitPolicy? RateLimitPolicy { get; set; }

    /// <summary>
    /// Điều hướng tới Chính sách CORS (toàn hệ thống hoặc theo tenant khi tenant có domain riêng) (qua <c>cors_policy_id</c>).
    /// </summary>
    public CorsPolicy? CorsPolicy { get; set; }

    /// <summary>
    /// Điều hướng tới Bộ security header gắn vào response (HSTS, CSP, X-Frame-Options (qua <c>security_header_policy_id</c>).
    /// </summary>
    public SecurityHeaderPolicy? SecurityHeaderPolicy { get; set; }

    #endregion
}
