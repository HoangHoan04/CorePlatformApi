// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Ghi đè hạn mức rate limit cho tenant cụ thể (gói cao cấp được nhiều request hơn)
/// </summary>
/// <remarks>Bảng <c>tenant_rate_limits</c> (phân hệ gateway).</remarks>
public class TenantRateLimit : BaseEntity
{
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
