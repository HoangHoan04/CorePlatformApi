// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Gán quyền cho vai trò kèm phạm vi dữ liệu (Data Scope) trên từng quyền
/// </summary>
/// <remarks>Bảng <c>role_permissions</c> (phân hệ iam).</remarks>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// Vai trò
    /// </summary>
    /// <remarks>FK → <c>iam.roles(id)</c></remarks>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Quyền
    /// </summary>
    /// <remarks>FK → <c>iam.permissions(id)</c></remarks>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Phạm vi: cá nhân, phòng ban, phòng ban + cấp dưới, chi nhánh, công ty, toàn tenant, tùy chọn
    /// </summary>
    /// <remarks>Ràng buộc: <c>data_scope IN ('OWN','DEPARTMENT','DEPARTMENT_TREE','BRANCH','COMPANY','ALL','CUSTOM')</c>; Mặc định DB: <c>'OWN'</c></remarks>
    public string DataScope { get; set; } = "OWN";

    /// <summary>
    /// Điều kiện ABAC bổ sung (VD: {"amount_lte": 50000000})
    /// </summary>
    public string? Conditions { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Vai trò (qua <c>role_id</c>).
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Quyền cụ thể = tài nguyên × hành động (qua <c>permission_id</c>).
    /// </summary>
    public Permission Permission { get; set; } = null!;

    /// <summary>
    /// Danh sách RolePermissionScope con (qua <c>iam.role_permission_scopes.role_permission_id</c>).
    /// </summary>
    public ICollection<RolePermissionScope> RolePermissionScopes { get; set; } = [];

    #endregion
}
