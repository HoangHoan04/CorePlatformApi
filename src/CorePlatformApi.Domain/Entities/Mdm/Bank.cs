// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Danh mục ngân hàng (dùng cho VietQR, chi lương, thanh toán NCC)
/// </summary>
/// <remarks>Bảng <c>banks</c> (phân hệ mdm).</remarks>
public class Bank : BaseEntity
{
    /// <summary>
    /// Quốc gia
    /// </summary>
    /// <remarks>Mặc định DB: <c>'VN'</c>; FK → <c>mdm.countries(code)</c></remarks>
    public string CountryCode { get; set; } = "VN";

    /// <summary>
    /// Mã viết tắt (VCB, TCB, MB...)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mã BIN NAPAS dùng tạo VietQR (970436...)
    /// </summary>
    public string? Bin { get; set; }

    /// <summary>
    /// Tên ngắn (Vietcombank)
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mã SWIFT/BIC
    /// </summary>
    public string? SwiftCode { get; set; }

    /// <summary>
    /// Logo ngân hàng
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Hỗ trợ VietQR hay không
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool SupportsVietqr { get; set; } = false;

    /// <summary>
    /// Còn sử dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục quốc gia theo chuẩn ISO 3166 (qua <c>country_code</c>).
    /// </summary>
    public Country Country { get; set; } = null!;

    #endregion
}
