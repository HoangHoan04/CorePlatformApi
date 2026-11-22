// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Danh mục hành động chuẩn (VIEW, CREATE, UPDATE, DELETE, APPROVE, EXPORT, IMPORT, PRINT...)
/// </summary>
/// <remarks>Bảng <c>iam.permission_actions</c>.</remarks>
public class PermissionAction : AuditableEntity
{
    /// <summary>
    /// Mã hành động
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị (Xem, Thêm, Sửa...)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Thứ tự hiển thị trên ma trận phân quyền
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;
}
