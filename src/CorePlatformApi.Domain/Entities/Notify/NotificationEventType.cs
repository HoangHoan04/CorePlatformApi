// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Loại sự kiện thông báo do các phân hệ khai báo (hrm.leave_request.submitted, wms.stock.low...)
/// </summary>
/// <remarks>Bảng <c>notify.event_types</c>.</remarks>
public class NotificationEventType : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Phân hệ phát sinh
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Mã sự kiện
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị trong cài đặt thông báo
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Kênh mặc định: IN_APP, PUSH, EMAIL, SMS, ZALO
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{IN_APP}'</c></remarks>
    public List<string> DefaultChannels { get; set; } = ["IN_APP"];

    /// <summary>
    /// Mức ưu tiên
    /// </summary>
    /// <remarks>Ràng buộc: <c>priority IN ('LOW','NORMAL','HIGH','URGENT')</c>; Mặc định DB: <c>'NORMAL'</c></remarks>
    public string Priority { get; set; } = "NORMAL";

    /// <summary>
    /// Bắt buộc nhận, user không được tắt (VD: reset mật khẩu)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsMandatory { get; set; } = false;

    /// <summary>
    /// Khai báo biến template, VD: [{"name":"employee_name","sample":"Nguyễn Văn A"}]
    /// </summary>
    public string? Variables { get; set; }

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module Module { get; set; } = null!;

    /// <summary>
    /// Danh sách UserNotificationPreference con (qua <c>notify.user_preferences.event_type_id</c>).
    /// </summary>
    public ICollection<UserNotificationPreference> UserNotificationPreferences { get; set; } = [];

    #endregion
}
