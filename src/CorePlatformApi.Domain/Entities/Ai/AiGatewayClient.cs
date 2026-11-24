// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Client nội bộ gọi AI Gateway (mỗi phân hệ/ứng dụng một client key) để định danh, rate limit, tính quota
/// </summary>
/// <remarks>Bảng <c>gateway_clients</c> (phân hệ ai).</remarks>
public class AiGatewayClient : BaseEntity
{
    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Tên client (hrm-api, crm-api)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tiền tố key hiển thị
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Hash key nội bộ
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Alias model được phép dùng (NULL = tất cả)
    /// </summary>
    public List<string>? AllowedAliases { get; set; }

    /// <summary>
    /// Rate limit RPM
    /// </summary>
    public int? RequestsPerMinute { get; set; }

    /// <summary>
    /// Rate limit TPM
    /// </summary>
    public int? TokensPerMinute { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','DISABLED','REVOKED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Dùng gần nhất
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

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
