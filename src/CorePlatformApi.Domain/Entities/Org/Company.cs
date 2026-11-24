// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Storage;

namespace CorePlatformApi.Domain.Entities.Org;

/// <summary>
/// Công ty con / pháp nhân thuộc tenant. Hỗ trợ công ty mẹ - con nhiều cấp qua parent_id
/// </summary>
/// <remarks>Bảng <c>companies</c> (phân hệ org); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class Company : BaseEntity
{
    /// <summary>
    /// Công ty mẹ (NULL nếu là công ty cấp cao nhất)
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Mã công ty (dùng trong quy tắc sinh mã {COMPANY})
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên giao dịch
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tên pháp lý trên giấy ĐKKD
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// Tên viết tắt
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>
    /// Mã số thuế
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Số giấy chứng nhận ĐKKD
    /// </summary>
    public string? BusinessRegNo { get; set; }

    /// <summary>
    /// Ngày cấp ĐKKD
    /// </summary>
    public DateOnly? BusinessRegDate { get; set; }

    /// <summary>
    /// Người đại diện pháp luật
    /// </summary>
    public string? LegalRepresentative { get; set; }

    /// <summary>
    /// Chức danh người đại diện
    /// </summary>
    public string? RepresentativeTitle { get; set; }

    /// <summary>
    /// Địa chỉ chi tiết (số nhà, đường)
    /// </summary>
    public string? AddressLine { get; set; }

    /// <summary>
    /// Phường/Xã
    /// </summary>
    /// <remarks>FK → <c>mdm.administrative_units(id)</c></remarks>
    public Guid? WardId { get; set; }

    /// <summary>
    /// Tỉnh/Thành phố
    /// </summary>
    /// <remarks>FK → <c>mdm.administrative_units(id)</c></remarks>
    public Guid? ProvinceId { get; set; }

    /// <summary>
    /// Quốc gia
    /// </summary>
    /// <remarks>Mặc định DB: <c>'VN'</c>; FK → <c>mdm.countries(code)</c></remarks>
    public string CountryCode { get; set; } = "VN";

    /// <summary>
    /// Điện thoại
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Website
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Logo (storage.files)
    /// </summary>
    public Guid? LogoFileId { get; set; }

    /// <summary>
    /// Tiền tệ hạch toán
    /// </summary>
    /// <remarks>Mặc định DB: <c>'VND'</c>; FK → <c>mdm.currencies(code)</c></remarks>
    public string BaseCurrencyCode { get; set; } = "VND";

    /// <summary>
    /// Tháng bắt đầu năm tài chính
    /// </summary>
    /// <remarks>Ràng buộc: <c>fiscal_year_start_month BETWEEN 1 AND 12</c>; Mặc định DB: <c>1</c></remarks>
    public short FiscalYearStartMonth { get; set; } = 1;

    /// <summary>
    /// Lịch làm việc mặc định (mdm.work_calendars)
    /// </summary>
    public Guid? WorkCalendarId { get; set; }

    /// <summary>
    /// Đường dẫn id từ gốc '/uuid1/uuid2/' để truy vấn cây
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Cấp trong cây công ty
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short Level { get; set; } = 1;

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','INACTIVE')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>parent_id</c>).
    /// </summary>
    public Company? Parent { get; set; }

    /// <summary>
    /// Điều hướng tới Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã) (qua <c>ward_id</c>).
    /// </summary>
    public AdministrativeUnit? Ward { get; set; }

    /// <summary>
    /// Điều hướng tới Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã) (qua <c>province_id</c>).
    /// </summary>
    public AdministrativeUnit? Province { get; set; }

    /// <summary>
    /// Điều hướng tới Danh mục quốc gia theo chuẩn ISO 3166 (qua <c>country_code</c>).
    /// </summary>
    public Country Country { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục tiền tệ theo ISO 4217 (qua <c>base_currency_code</c>).
    /// </summary>
    public Currency BaseCurrency { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>logo_file_id</c>).
    /// </summary>
    public StorageFile? LogoFile { get; set; }

    /// <summary>
    /// Điều hướng tới Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc (qua <c>work_calendar_id</c>).
    /// </summary>
    public WorkCalendar? WorkCalendar { get; set; }

    /// <summary>
    /// Danh sách Company con (qua <c>org.companies.parent_id</c>).
    /// </summary>
    public ICollection<Company> Children { get; set; } = [];

    #endregion
}
