// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Danh mục tiền tệ theo ISO 4217
/// </summary>
/// <remarks>Bảng <c>currencies</c> (phân hệ mdm).</remarks>
public class Currency : BaseEntity
{
    /// <summary>
    /// Mã ISO 4217 (VND, USD, EUR...)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mã số ISO (704 = VND)
    /// </summary>
    public string? NumericCode { get; set; }

    /// <summary>
    /// Tên tiền tệ (Đồng Việt Nam)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ký hiệu hiển thị (₫, $)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Số chữ số thập phân khi hiển thị/làm tròn (VND = 0, USD = 2)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short DecimalPlaces { get; set; } = 0;

    /// <summary>
    /// Đơn vị làm tròn tối thiểu (VD: 1, 100, 1000 VND)
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public decimal RoundingUnit { get; set; } = 1m;

    /// <summary>
    /// Vị trí ký hiệu khi hiển thị
    /// </summary>
    /// <remarks>Ràng buộc: <c>symbol_position IN ('PREFIX','SUFFIX')</c>; Mặc định DB: <c>'SUFFIX'</c></remarks>
    public string SymbolPosition { get; set; } = "SUFFIX";

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
