// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Phân hệ được kích hoạt cho từng tenant (gói thuê bao). User chỉ vào được phân hệ tenant đã mua
/// </summary>
/// <remarks>Bảng <c>tenant_modules</c> (phân hệ iam).</remarks>
public class TenantModule : BaseEntity
{
    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Đang bật
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Ngày bắt đầu sử dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Ngày hết hạn (NULL = vô thời hạn)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Giới hạn số user dùng phân hệ này
    /// </summary>
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Cấu hình riêng phân hệ cho tenant
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Settings { get; set; } = "{}";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module Module { get; set; } = null!;

    #endregion
}
