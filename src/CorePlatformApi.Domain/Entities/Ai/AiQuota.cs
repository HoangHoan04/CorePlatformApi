// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Hạn mức sử dụng AI theo tenant / phân hệ / user theo ngày hoặc tháng
/// </summary>
/// <remarks>Bảng <c>ai.quotas</c>.</remarks>
public class AiQuota : AuditableEntity, IMustHaveTenant
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
    /// Phân hệ (NULL = toàn tenant)
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// User (NULL = không giới hạn theo user)
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Chu kỳ
    /// </summary>
    /// <remarks>Ràng buộc: <c>period IN ('DAILY','MONTHLY')</c></remarks>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// Số request tối đa
    /// </summary>
    public int? MaxRequests { get; set; }

    /// <summary>
    /// Tổng token tối đa
    /// </summary>
    public long? MaxTokens { get; set; }

    /// <summary>
    /// Chi phí tối đa (USD)
    /// </summary>
    public decimal? MaxCostUsd { get; set; }

    /// <summary>
    /// Ngưỡng % gửi cảnh báo
    /// </summary>
    /// <remarks>Ràng buộc: <c>alert_threshold_pct BETWEEN 1 AND 100</c>; Mặc định DB: <c>80</c></remarks>
    public short AlertThresholdPct { get; set; } = 80;

    /// <summary>
    /// Khi vượt: chặn, chỉ cảnh báo, chuyển model rẻ hơn
    /// </summary>
    /// <remarks>Ràng buộc: <c>action_on_exceed IN ('BLOCK','ALERT_ONLY','DOWNGRADE')</c>; Mặc định DB: <c>'BLOCK'</c></remarks>
    public string ActionOnExceed { get; set; } = "BLOCK";

    /// <summary>
    /// Alias rẻ hơn khi DOWNGRADE
    /// </summary>
    /// <remarks>FK → <c>ai.model_aliases(id)</c></remarks>
    public Guid? DowngradeAliasId { get; set; }

    /// <summary>
    /// Đang áp dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Điều hướng tới Alias model logic (default-chat, cheap-summary, embedding-default) để đổi model không cần sửa code; hỗ trợ fallback (qua <c>downgrade_alias_id</c>).
    /// </summary>
    public AiModelAlias? DowngradeAlias { get; set; }

    #endregion
}
