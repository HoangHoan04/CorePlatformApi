// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Iam;

/// <summary>
/// Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI...). Dùng cho phân quyền theo phân hệ và routing
/// </summary>
/// <remarks>Bảng <c>iam.modules</c>.</remarks>
public class Module : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã phân hệ (CORE, HRM, CRM, WMS, AI)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên phân hệ
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tiền tố route trên gateway (/hrm)
    /// </summary>
    public string? BaseRoute { get; set; }

    /// <summary>
    /// Icon menu
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Màu nhận diện
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// URL ứng dụng web của phân hệ
    /// </summary>
    public string? WebUrl { get; set; }

    /// <summary>
    /// Phân hệ lõi luôn bật cho mọi tenant
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsCore { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Còn sử dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;
}
