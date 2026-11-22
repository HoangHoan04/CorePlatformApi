// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Ghi đè hạn mức rate limit cho tenant cụ thể (gói cao cấp được nhiều request hơn)
/// </summary>
/// <remarks>Bảng <c>gateway.tenant_rate_limits</c>.</remarks>
public class TenantRateLimit : AuditableEntity, IMustHaveTenant
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
    /// Chính sách gốc
    /// </summary>
    /// <remarks>FK → <c>gateway.rate_limit_policies(id)</c></remarks>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Hạn mức riêng
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Cửa sổ riêng
    /// </summary>
    public int? WindowSeconds { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chính sách rate limit/throttling theo IP, user, tenant, client, API key (qua <c>policy_id</c>).
    /// </summary>
    public RateLimitPolicy Policy { get; set; } = null!;

    #endregion
}
