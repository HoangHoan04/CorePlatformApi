// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Biểu thuế suất có hiệu lực theo thời gian (VD: VAT giảm 8% theo từng nghị định)
/// </summary>
/// <remarks>Bảng <c>mdm.tax_rates</c>.</remarks>
public class TaxRate : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = biểu thuế chuẩn)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Loại thuế
    /// </summary>
    /// <remarks>FK → <c>mdm.tax_types(code)</c></remarks>
    public string TaxTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Mã thuế suất (VAT10, VAT8, VAT5, VAT0, KCT, KKKNT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị (Thuế GTGT 10%)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Thuế suất % (NULL với KCT/KKKNT không tính thuế)
    /// </summary>
    /// <remarks>Ràng buộc: <c>rate &gt;= 0</c></remarks>
    public decimal? Rate { get; set; }

    /// <summary>
    /// Cách tính: theo %, không chịu thuế, không kê khai, miễn thuế
    /// </summary>
    /// <remarks>Ràng buộc: <c>calc_method IN ('PERCENT','NOT_SUBJECT','NOT_DECLARED','EXEMPT')</c>; Mặc định DB: <c>'PERCENT'</c></remarks>
    public string CalcMethod { get; set; } = "PERCENT";

    /// <summary>
    /// Mã thuế suất trên hóa đơn điện tử (10%, 8%, KCT, KKKNT)
    /// </summary>
    public string? EinvoiceCode { get; set; }

    /// <summary>
    /// Hiệu lực từ
    /// </summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>
    /// Hiệu lực đến (NULL = còn hiệu lực)
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Văn bản pháp lý
    /// </summary>
    public string? LegalReference { get; set; }

    /// <summary>
    /// Mặc định khi tạo chứng từ
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Loại thuế (GTGT, TNDN, TNCN, Nhập khẩu, Tiêu thụ đặc biệt (qua <c>tax_type_code</c>).
    /// </summary>
    public TaxType TaxType { get; set; } = null!;

    #endregion
}
