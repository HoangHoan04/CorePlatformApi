// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Danh mục quốc gia theo chuẩn ISO 3166
/// </summary>
/// <remarks>Bảng <c>mdm.countries</c>.</remarks>
public class Country : AuditableEntity
{
    /// <summary>
    /// Mã ISO 3166-1 alpha-2 (VN, US, JP...)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mã ISO alpha-3 (VNM, USA...)
    /// </summary>
    public string Alpha3Code { get; set; } = string.Empty;

    /// <summary>
    /// Mã số ISO (704 = Việt Nam)
    /// </summary>
    public string? NumericCode { get; set; }

    /// <summary>
    /// Tên tiếng Việt
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tên tiếng Anh
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Mã vùng điện thoại quốc tế (+84)
    /// </summary>
    public string? PhoneCode { get; set; }

    /// <summary>
    /// Tiền tệ mặc định của quốc gia (VND)
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Cờ dạng emoji để hiển thị nhanh
    /// </summary>
    public string? FlagEmoji { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Còn sử dụng hay không
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;
}
