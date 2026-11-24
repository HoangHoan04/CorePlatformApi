// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Ngoại lệ của lịch: nghỉ riêng công ty, ngày làm bù, ngày đổi ca... áp dụng cho cả lịch hoặc riêng chi nhánh
/// </summary>
/// <remarks>Bảng <c>work_calendar_exceptions</c> (phân hệ mdm); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class WorkCalendarException : BaseEntity
{
    /// <summary>
    /// Lịch
    /// </summary>
    /// <remarks>FK → <c>mdm.work_calendars(id)</c></remarks>
    public Guid CalendarId { get; set; }

    /// <summary>
    /// Chỉ áp dụng cho chi nhánh này (NULL = toàn lịch)
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Ngày ngoại lệ
    /// </summary>
    public DateOnly ExceptionDate { get; set; }

    /// <summary>
    /// Loại: nghỉ, làm bù, nửa ngày, giờ đặc biệt
    /// </summary>
    /// <remarks>Ràng buộc: <c>exception_type IN ('DAY_OFF','MAKEUP_WORKDAY','HALF_DAY','CUSTOM_HOURS')</c></remarks>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// Liên kết ngày lễ (nếu do lễ)
    /// </summary>
    /// <remarks>FK → <c>mdm.holidays(id)</c></remarks>
    public Guid? HolidayId { get; set; }

    /// <summary>
    /// Tên/lý do
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nghỉ có lương
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsPaid { get; set; } = true;

    /// <summary>
    /// Giờ bắt đầu (CUSTOM_HOURS)
    /// </summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc
    /// </summary>
    public TimeOnly? EndTime { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc (qua <c>calendar_id</c>).
    /// </summary>
    public WorkCalendar Calendar { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Điều hướng tới Ngày lễ quốc gia (tenant_id NULL) và ngày lễ/nghỉ riêng của tenant (qua <c>holiday_id</c>).
    /// </summary>
    public Holiday? Holiday { get; set; }

    #endregion
}
