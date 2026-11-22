// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Gán user vào nhiều công ty/chi nhánh/phòng ban (kiêm nhiệm). Dùng để tính Data Scope và chọn ngữ cảnh làm việc
/// </summary>
/// <remarks>Bảng <c>iam.user_org_assignments</c>.</remarks>
public class UserOrgAssignment : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Công ty
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Chi nhánh
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Phòng ban
    /// </summary>
    /// <remarks>FK → <c>org.departments(id)</c></remarks>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Chức danh tại đơn vị này
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Đơn vị chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Là quản lý của đơn vị này
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsManager { get; set; } = false;

    /// <summary>
    /// Hiệu lực từ ngày
    /// </summary>
    /// <remarks>Mặc định DB: <c>current_date</c></remarks>
    public DateOnly ValidFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Hiệu lực đến ngày (NULL = vô thời hạn)
    /// </summary>
    public DateOnly? ValidTo { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Điều hướng tới Phòng ban / bộ phận / tổ nhóm dạng cây (qua <c>department_id</c>).
    /// </summary>
    public Department? Department { get; set; }

    #endregion
}
