// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Vai trò. tenant_id NULL = vai trò mẫu toàn hệ thống (template) để tenant sao chép
/// </summary>
/// <remarks>Bảng <c>roles</c> (phân hệ iam); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class Role : BaseEntity
{
    /// <summary>
    /// Vai trò riêng của một công ty (NULL = dùng toàn tenant)
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Mã vai trò (HR_MANAGER, WAREHOUSE_KEEPER)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên vai trò
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Vai trò hệ thống (Super Admin, Tenant Admin) — không cho sửa/xóa
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Tự gán cho user mới
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Độ ưu tiên khi hợp nhất data scope (số lớn ưu tiên)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','INACTIVE')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Danh sách RolePermission con (qua <c>iam.role_permissions.role_id</c>).
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];

    /// <summary>
    /// Danh sách UserRole con (qua <c>iam.user_roles.role_id</c>).
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];

    #endregion
}
