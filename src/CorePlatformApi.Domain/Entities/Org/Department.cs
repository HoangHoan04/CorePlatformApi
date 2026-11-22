// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Org;

/// <summary>
/// Phòng ban / bộ phận / tổ nhóm dạng cây. Dùng cho Data Scope DEPARTMENT / DEPARTMENT_TREE
/// </summary>
/// <remarks>Bảng <c>org.departments</c>.</remarks>
public class Department : SoftDeletableEntity, IMustHaveTenant
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
    /// Công ty
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Chi nhánh (NULL nếu phòng ban cấp công ty)
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Phòng ban cha
    /// </summary>
    /// <remarks>FK → <c>org.departments(id)</c></remarks>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Mã phòng ban
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên phòng ban
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại: khối, phòng, tổ, nhóm
    /// </summary>
    /// <remarks>Ràng buộc: <c>dept_type IN ('DIVISION','DEPARTMENT','TEAM','GROUP')</c>; Mặc định DB: <c>'DEPARTMENT'</c></remarks>
    public string DeptType { get; set; } = "DEPARTMENT";

    /// <summary>
    /// Trưởng bộ phận (iam.users) — dùng cho workflow approver DEPARTMENT_MANAGER
    /// </summary>
    public Guid? ManagerUserId { get; set; }

    /// <summary>
    /// Phó bộ phận
    /// </summary>
    public Guid? DeputyUserId { get; set; }

    /// <summary>
    /// Mã trung tâm chi phí (đối soát kế toán)
    /// </summary>
    public string? CostCenterCode { get; set; }

    /// <summary>
    /// Đường dẫn id '/root/child/' — truy vấn cây con bằng LIKE path || '%'
    /// </summary>
    /// <remarks>Mặc định DB: <c>'/'</c></remarks>
    public string Path { get; set; } = "/";

    /// <summary>
    /// Cấp trong cây
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short Level { get; set; } = 1;

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','INACTIVE')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Điều hướng tới Phòng ban / bộ phận / tổ nhóm dạng cây (qua <c>parent_id</c>).
    /// </summary>
    public Department? Parent { get; set; }

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>manager_user_id</c>).
    /// </summary>
    public User? ManagerUser { get; set; }

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>deputy_user_id</c>).
    /// </summary>
    public User? DeputyUser { get; set; }

    /// <summary>
    /// Danh sách Department con (qua <c>org.departments.parent_id</c>).
    /// </summary>
    public ICollection<Department> Children { get; set; } = [];

    #endregion
}
