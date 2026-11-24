// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Tài nguyên/chức năng (menu, màn hình, API) của từng phân hệ dạng cây. Permission = resource + action
/// </summary>
/// <remarks>Bảng <c>resources</c> (phân hệ iam).</remarks>
public class Resource : BaseEntity
{
    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Tài nguyên cha (nhóm menu)
    /// </summary>
    /// <remarks>FK → <c>iam.resources(id)</c></remarks>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Mã tài nguyên (EMPLOYEE, LEAVE_REQUEST, GOODS_RECEIPT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại: nhóm menu, chức năng, báo cáo, API thuần
    /// </summary>
    /// <remarks>Ràng buộc: <c>resource_type IN ('MENU_GROUP','FUNCTION','REPORT','API')</c>; Mặc định DB: <c>'FUNCTION'</c></remarks>
    public string ResourceType { get; set; } = "FUNCTION";

    /// <summary>
    /// Route frontend (/hrm/employees)
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Icon menu
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Tài nguyên có áp dụng phạm vi dữ liệu không
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool SupportsDataScope { get; set; } = true;

    /// <summary>
    /// Thứ tự menu
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Hiển thị trên menu
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Còn sử dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module Module { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài nguyên/chức năng (menu, màn hình, API) của từng phân hệ dạng cây (qua <c>parent_id</c>).
    /// </summary>
    public Resource? Parent { get; set; }

    /// <summary>
    /// Danh sách Resource con (qua <c>iam.resources.parent_id</c>).
    /// </summary>
    public ICollection<Resource> Children { get; set; } = [];

    #endregion
}
