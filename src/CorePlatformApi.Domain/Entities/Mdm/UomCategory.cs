// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Nhóm đơn vị tính (Số lượng, Khối lượng, Thể tích, Chiều dài, Diện tích, Thời gian)
/// </summary>
/// <remarks>Bảng <c>mdm.uom_categories</c>.</remarks>
public class UomCategory : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã nhóm (COUNT, WEIGHT, VOLUME, LENGTH, AREA, TIME)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên nhóm
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
