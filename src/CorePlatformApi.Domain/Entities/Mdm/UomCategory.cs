// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Nhóm đơn vị tính (Số lượng, Khối lượng, Thể tích, Chiều dài, Diện tích, Thời gian)
/// </summary>
/// <remarks>Bảng <c>uom_categories</c> (phân hệ mdm).</remarks>
public class UomCategory : BaseEntity
{
    /// <summary>
    /// Mã nhóm (COUNT, WEIGHT, VOLUME, LENGTH, AREA, TIME)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên nhóm
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
