// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Quyền gán trực tiếp cho user (cộng thêm hoặc loại trừ so với vai trò)
/// </summary>
/// <remarks>Bảng <c>user_permission_overrides</c> (phân hệ iam).</remarks>
public class UserPermissionOverride : BaseEntity
{
    /// <summary>
    /// User
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Quyền
    /// </summary>
    /// <remarks>FK → <c>iam.permissions(id)</c></remarks>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// GRANT = cấp thêm, DENY = chặn (ưu tiên cao nhất)
    /// </summary>
    /// <remarks>Ràng buộc: <c>effect IN ('GRANT','DENY')</c>; Mặc định DB: <c>'GRANT'</c></remarks>
    public string Effect { get; set; } = "GRANT";

    /// <summary>
    /// Phạm vi dữ liệu khi GRANT
    /// </summary>
    /// <remarks>Ràng buộc: <c>data_scope IN ('OWN','DEPARTMENT','DEPARTMENT_TREE','BRANCH','COMPANY','ALL')</c></remarks>
    public string? DataScope { get; set; }

    /// <summary>
    /// Hết hạn
    /// </summary>
    public DateTimeOffset? ValidTo { get; set; }

    /// <summary>
    /// Lý do
    /// </summary>
    public string? Reason { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Quyền cụ thể = tài nguyên × hành động (qua <c>permission_id</c>).
    /// </summary>
    public Permission Permission { get; set; } = null!;

    #endregion
}
