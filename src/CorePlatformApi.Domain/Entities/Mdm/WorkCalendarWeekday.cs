// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Mẫu ngày làm việc trong tuần của lịch (T2..CN, giờ vào/ra, nghỉ trưa)
/// </summary>
/// <remarks>Bảng <c>mdm.work_calendar_weekdays</c>.</remarks>
public class WorkCalendarWeekday : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Lịch
    /// </summary>
    /// <remarks>FK → <c>mdm.work_calendars(id)</c></remarks>
    public Guid CalendarId { get; set; }

    /// <summary>
    /// Thứ theo ISO: 1 = Thứ Hai ... 7 = Chủ Nhật
    /// </summary>
    /// <remarks>Ràng buộc: <c>day_of_week BETWEEN 1 AND 7</c></remarks>
    public short DayOfWeek { get; set; }

    /// <summary>
    /// Có làm việc không
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsWorkingDay { get; set; } = true;

    /// <summary>
    /// Tỷ lệ công (1 = cả ngày, 0.5 = nửa ngày T7)
    /// </summary>
    /// <remarks>Ràng buộc: <c>work_ratio BETWEEN 0 AND 1</c>; Mặc định DB: <c>1</c></remarks>
    public decimal WorkRatio { get; set; } = 1m;

    /// <summary>
    /// Giờ bắt đầu
    /// </summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc
    /// </summary>
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// Bắt đầu nghỉ trưa
    /// </summary>
    public TimeOnly? BreakStart { get; set; }

    /// <summary>
    /// Kết thúc nghỉ trưa
    /// </summary>
    public TimeOnly? BreakEnd { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc (qua <c>calendar_id</c>).
    /// </summary>
    public WorkCalendar Calendar { get; set; } = null!;

    #endregion
}
