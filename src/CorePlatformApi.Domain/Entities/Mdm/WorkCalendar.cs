// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc
/// </summary>
/// <remarks>Bảng <c>mdm.work_calendars</c>.</remarks>
public class WorkCalendar : SoftDeletableEntity, IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Công ty áp dụng (NULL = toàn tenant)
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Mã lịch (HC_T2_T6, HC_T2_T7_SANG)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên lịch
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Múi giờ
    /// </summary>
    /// <remarks>Mặc định DB: <c>'Asia/Ho_Chi_Minh'</c></remarks>
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Số giờ công chuẩn/ngày
    /// </summary>
    /// <remarks>Mặc định DB: <c>8</c></remarks>
    public decimal StandardHoursPerDay { get; set; } = 8m;

    /// <summary>
    /// Tự áp dụng lễ quốc gia từ mdm.holidays
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool ApplyPublicHolidays { get; set; } = true;

    /// <summary>
    /// Lịch mặc định của tenant
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','INACTIVE')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Danh sách WorkCalendarWeekday con (qua <c>mdm.work_calendar_weekdays.calendar_id</c>).
    /// </summary>
    public ICollection<WorkCalendarWeekday> WorkCalendarWeekdays { get; set; } = [];

    /// <summary>
    /// Danh sách WorkCalendarException con (qua <c>mdm.work_calendar_exceptions.calendar_id</c>).
    /// </summary>
    public ICollection<WorkCalendarException> WorkCalendarExceptions { get; set; } = [];

    #endregion
}
