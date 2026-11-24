// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Quyền cụ thể = tài nguyên × hành động. Mã quyền dạng MODULE:RESOURCE:ACTION (HRM:EMPLOYEE:CREATE)
/// </summary>
/// <remarks>Bảng <c>permissions</c> (phân hệ iam).</remarks>
public class Permission : BaseEntity
{
    /// <summary>
    /// Phân hệ (denormalize để lọc nhanh)
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Tài nguyên
    /// </summary>
    /// <remarks>FK → <c>iam.resources(id)</c></remarks>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Hành động
    /// </summary>
    /// <remarks>FK → <c>iam.permission_actions(code)</c></remarks>
    public string ActionCode { get; set; } = string.Empty;

    /// <summary>
    /// Mã quyền đầy đủ đưa vào token/policy (HRM:EMPLOYEE:VIEW)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên quyền
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Quyền do hệ thống khai báo (seed từ code), không cho xóa
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsSystem { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module Module { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài nguyên/chức năng (menu, màn hình, API) của từng phân hệ dạng cây (qua <c>resource_id</c>).
    /// </summary>
    public Resource Resource { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục hành động chuẩn (VIEW, CREATE, UPDATE, DELETE, APPROVE, EXPORT, IMPORT, PRINT (qua <c>action_code</c>).
    /// </summary>
    public PermissionAction Action { get; set; } = null!;

    /// <summary>
    /// Danh sách RolePermission con (qua <c>iam.role_permissions.permission_id</c>).
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    /// <summary>
    /// Danh sách UserPermissionOverride con (qua <c>iam.user_permission_overrides.permission_id</c>).
    /// </summary>
    public ICollection<UserPermissionOverride> UserPermissionOverrides { get; set; } = [];

    #endregion
}
