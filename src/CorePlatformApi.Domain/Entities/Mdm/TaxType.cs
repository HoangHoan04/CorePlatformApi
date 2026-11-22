// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Loại thuế (GTGT, TNDN, TNCN, Nhập khẩu, Tiêu thụ đặc biệt...)
/// </summary>
/// <remarks>Bảng <c>mdm.tax_types</c>.</remarks>
public class TaxType : AuditableEntity
{
    /// <summary>
    /// Mã loại thuế (VAT, CIT, PIT, IMPORT, SCT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên loại thuế
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;
}
