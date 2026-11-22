// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Danh sách đơn vị cụ thể khi data_scope = CUSTOM (VD: chỉ xem chi nhánh HN01 và HN02)
/// </summary>
/// <remarks>Bảng <c>iam.role_permission_scopes</c>.</remarks>
public class RolePermissionScope : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Quyền của vai trò
    /// </summary>
    /// <remarks>FK → <c>iam.role_permissions(id)</c></remarks>
    public Guid RolePermissionId { get; set; }

    /// <summary>
    /// Loại đơn vị
    /// </summary>
    /// <remarks>Ràng buộc: <c>scope_type IN ('COMPANY','BRANCH','DEPARTMENT')</c></remarks>
    public string ScopeType { get; set; } = string.Empty;

    /// <summary>
    /// Id công ty / chi nhánh / phòng ban
    /// </summary>
    public Guid ScopeId { get; set; }

    /// <summary>
    /// Bao gồm đơn vị con
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IncludeChildren { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Gán quyền cho vai trò kèm phạm vi dữ liệu (Data Scope) trên từng quyền (qua <c>role_permission_id</c>).
    /// </summary>
    public RolePermission RolePermission { get; set; } = null!;

    #endregion
}
