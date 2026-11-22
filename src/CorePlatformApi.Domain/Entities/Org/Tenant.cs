// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Storage;

namespace CorePlatformApi.Domain.Entities.Org;

/// <summary>
/// Tenant = Tập đoàn/khách hàng thuê hệ thống. Mọi dữ liệu nghiệp vụ đều gắn tenant_id để cô lập dữ liệu
/// </summary>
/// <remarks>Bảng <c>org.tenants</c>.</remarks>
public class Tenant : SoftDeletableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã tenant duy nhất, dùng trong subdomain/đăng nhập (VD: remak)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tên pháp lý đầy đủ
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// Mã số thuế của tập đoàn/công ty mẹ
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Logo (storage.files)
    /// </summary>
    public Guid? LogoFileId { get; set; }

    /// <summary>
    /// Domain chính (erp.congty.vn)
    /// </summary>
    public string? PrimaryDomain { get; set; }

    /// <summary>
    /// Subdomain trên nền tảng (remak.baseerp.vn)
    /// </summary>
    public string? Subdomain { get; set; }

    /// <summary>
    /// Người liên hệ chính
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Email liên hệ
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Quốc gia hoạt động chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>'VN'</c>; FK → <c>mdm.countries(code)</c></remarks>
    public string CountryCode { get; set; } = "VN";

    /// <summary>
    /// Múi giờ mặc định (IANA)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'Asia/Ho_Chi_Minh'</c></remarks>
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Ngôn ngữ mặc định
    /// </summary>
    /// <remarks>Mặc định DB: <c>'vi-VN'</c></remarks>
    public string DefaultLocale { get; set; } = "vi-VN";

    /// <summary>
    /// Tiền tệ hạch toán mặc định
    /// </summary>
    /// <remarks>Mặc định DB: <c>'VND'</c>; FK → <c>mdm.currencies(code)</c></remarks>
    public string DefaultCurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Gói dịch vụ (BASIC, PRO, ENTERPRISE)
    /// </summary>
    public string? PlanCode { get; set; }

    /// <summary>
    /// Số user tối đa theo gói; NULL = không giới hạn
    /// </summary>
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Dung lượng lưu trữ tối đa (MB)
    /// </summary>
    public long? MaxStorageMb { get; set; }

    /// <summary>
    /// Trạng thái tenant
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('TRIAL','ACTIVE','SUSPENDED','EXPIRED','CLOSED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Hết hạn dùng thử
    /// </summary>
    public DateTimeOffset? TrialEndsAt { get; set; }

    /// <summary>
    /// Hết hạn hợp đồng/thuê bao
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Mô hình cô lập dữ liệu ở các phân hệ: dùng chung DB, schema riêng, DB riêng
    /// </summary>
    /// <remarks>Ràng buộc: <c>isolation_mode IN ('SHARED','SCHEMA','DATABASE')</c>; Mặc định DB: <c>'SHARED'</c></remarks>
    public string IsolationMode { get; set; } = "SHARED";

    /// <summary>
    /// Thông tin kết nối DB riêng (đã mã hóa) khi isolation_mode = DATABASE
    /// </summary>
    public string? ConnectionInfo { get; set; }

    /// <summary>
    /// Thiết lập mở rộng (theme, màu thương hiệu...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Settings { get; set; } = "{}";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục quốc gia theo chuẩn ISO 3166 (qua <c>country_code</c>).
    /// </summary>
    public Country Country { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục tiền tệ theo ISO 4217 (qua <c>default_currency_code</c>).
    /// </summary>
    public Currency DefaultCurrency { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>logo_file_id</c>).
    /// </summary>
    public StorageFile? LogoFile { get; set; }

    #endregion
}
