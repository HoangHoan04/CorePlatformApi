// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Tỷ giá quy đổi theo thời điểm
/// </summary>
/// <remarks>Bảng <c>mdm.exchange_rates</c>.</remarks>
public class ExchangeRate : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = tỷ giá hệ thống đồng bộ tự động)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Tiền tệ nguồn (USD)
    /// </summary>
    /// <remarks>FK → <c>mdm.currencies(code)</c></remarks>
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Tiền tệ đích (VND)
    /// </summary>
    /// <remarks>FK → <c>mdm.currencies(code)</c></remarks>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Loại tỷ giá: mua TM, mua CK, bán, chuyển khoản, trung tâm NHNN, hạch toán
    /// </summary>
    /// <remarks>Ràng buộc: <c>rate_type IN ('BUY_CASH','BUY_TRANSFER','SELL','TRANSFER','CENTRAL','ACCOUNTING')</c>; Mặc định DB: <c>'TRANSFER'</c></remarks>
    public string RateType { get; set; } = "TRANSFER";

    /// <summary>
    /// Tỷ giá: 1 from_currency = rate to_currency
    /// </summary>
    /// <remarks>Ràng buộc: <c>rate &gt; 0</c></remarks>
    public decimal Rate { get; set; }

    /// <summary>
    /// Thời điểm áp dụng
    /// </summary>
    public DateTimeOffset EffectiveAt { get; set; }

    /// <summary>
    /// Nguồn: VCB, SBV, MANUAL...
    /// </summary>
    /// <remarks>Mặc định DB: <c>'MANUAL'</c></remarks>
    public string Source { get; set; } = "MANUAL";

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Note { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Danh mục tiền tệ theo ISO 4217 (qua <c>from_currency</c>).
    /// </summary>
    public Currency FromCurrencyNavigation { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục tiền tệ theo ISO 4217 (qua <c>to_currency</c>).
    /// </summary>
    public Currency ToCurrencyNavigation { get; set; } = null!;

    #endregion
}
