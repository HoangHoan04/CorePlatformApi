// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Gán vai trò cho user, có thể giới hạn vai trò trong một công ty/chi nhánh và có thời hạn
/// </summary>
/// <remarks>Bảng <c>iam.user_roles</c>.</remarks>
public class UserRole : AuditableEntity
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
    /// Vai trò
    /// </summary>
    /// <remarks>FK → <c>iam.roles(id)</c></remarks>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Vai trò chỉ có hiệu lực trong công ty này (NULL = mọi công ty)
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Vai trò chỉ có hiệu lực trong chi nhánh này
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Hiệu lực từ
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Hiệu lực đến (gán quyền tạm thời)
    /// </summary>
    public DateTimeOffset? ValidTo { get; set; }

    /// <summary>
    /// Người cấp quyền
    /// </summary>
    public Guid? GrantedBy { get; set; }

    /// <summary>
    /// Ghi chú lý do cấp
    /// </summary>
    public string? Note { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Vai trò (qua <c>role_id</c>).
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    #endregion
}
