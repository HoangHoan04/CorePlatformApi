// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Danh mục scope API cấp cho client/API key (hrm.read, wms.write, openid...)
/// </summary>
/// <remarks>Bảng <c>api_scopes</c> (phân hệ iam).</remarks>
public class ApiScope : BaseEntity
{
    /// <summary>
    /// Mã scope (hrm.employee.read)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả quyền truy cập
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Scope nhạy cảm (lương, hợp đồng) cần duyệt khi cấp
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsSensitive { get; set; } = false;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    #endregion
}
