// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Job;

/// <summary>
/// Ghi đè lịch/tham số job theo tenant (VD: tenant A chốt công lúc 23:00, tenant B lúc 01:00)
/// </summary>
/// <remarks>Bảng <c>tenant_overrides</c> (phân hệ job).</remarks>
public class JobTenantOverride : BaseEntity
{
    /// <summary>
    /// Job
    /// </summary>
    /// <remarks>FK → <c>job.definitions(id)</c></remarks>
    public Guid JobId { get; set; }

    /// <summary>
    /// Cron riêng
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Tham số riêng (merge với mặc định)
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Bật/tắt cho tenant
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsEnabled { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục job định kỳ toàn hệ thống (chốt công cuối ngày, cảnh báo tồn kho, dọn file rác, đồng bộ tỷ giá (qua <c>job_id</c>).
    /// </summary>
    public JobDefinition Job { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    #endregion
}
