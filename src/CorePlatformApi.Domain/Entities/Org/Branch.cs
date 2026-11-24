// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;

namespace CorePlatformApi.Domain.Entities.Org;

/// <summary>
/// Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty
/// </summary>
/// <remarks>Bảng <c>branches</c> (phân hệ org); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class Branch : BaseEntity
{
    /// <summary>
    /// Công ty sở hữu chi nhánh
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Chi nhánh cấp trên (VD: vùng → cửa hàng)
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Mã chi nhánh (dùng trong sinh mã {BRANCH}, VD: HN01)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên chi nhánh
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại: trụ sở, văn phòng, cửa hàng, kho, nhà máy, vùng
    /// </summary>
    /// <remarks>Ràng buộc: <c>branch_type IN ('HEAD_OFFICE','OFFICE','STORE','WAREHOUSE','FACTORY','REGION','OTHER')</c>; Mặc định DB: <c>'OFFICE'</c></remarks>
    public string BranchType { get; set; } = "OFFICE";

    /// <summary>
    /// MST chi nhánh (nếu hạch toán phụ thuộc có MST 13 số)
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Địa chỉ chi tiết
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
    /// Vĩ độ (dùng cho chấm công GPS, giao hàng)
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Kinh độ
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Bán kính geofence (mét) cho chấm công mobile
    /// </summary>
    public int? GeofenceRadiusM { get; set; }

    /// <summary>
    /// Điện thoại
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Người quản lý chi nhánh (iam.users)
    /// </summary>
    public Guid? ManagerUserId { get; set; }

    /// <summary>
    /// Múi giờ riêng (NULL = theo tenant)
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Lịch làm việc riêng (NULL = theo công ty)
    /// </summary>
    public Guid? WorkCalendarId { get; set; }

    /// <summary>
    /// Ngày khai trương/thành lập
    /// </summary>
    public DateOnly? OpeningDate { get; set; }

    /// <summary>
    /// Ngày đóng cửa
    /// </summary>
    public DateOnly? ClosingDate { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','INACTIVE','CLOSED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>parent_id</c>).
    /// </summary>
    public Branch? Parent { get; set; }

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
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>manager_user_id</c>).
    /// </summary>
    public User? ManagerUser { get; set; }

    /// <summary>
    /// Điều hướng tới Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc (qua <c>work_calendar_id</c>).
    /// </summary>
    public WorkCalendar? WorkCalendar { get; set; }

    /// <summary>
    /// Danh sách Branch con (qua <c>org.branches.parent_id</c>).
    /// </summary>
    public ICollection<Branch> Children { get; set; } = [];

    #endregion
}
