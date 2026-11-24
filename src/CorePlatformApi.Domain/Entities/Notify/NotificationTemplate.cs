// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Notify;

/// <summary>
/// Mẫu nội dung theo sự kiện × kênh × ngôn ngữ, dùng biến {{employee_name}}. Tenant có thể ghi đè mẫu hệ thống
/// </summary>
/// <remarks>Bảng <c>templates</c> (phân hệ notify); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class NotificationTemplate : BaseEntity
{
    /// <summary>
    /// Sự kiện
    /// </summary>
    /// <remarks>FK → <c>notify.event_types(id)</c></remarks>
    public Guid EventTypeId { get; set; }

    /// <summary>
    /// Kênh
    /// </summary>
    /// <remarks>Ràng buộc: <c>channel IN ('IN_APP','PUSH','EMAIL','SMS','ZALO')</c></remarks>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Ngôn ngữ
    /// </summary>
    /// <remarks>Mặc định DB: <c>'vi-VN'</c></remarks>
    public string Locale { get; set; } = "vi-VN";

    /// <summary>
    /// Tiêu đề (email) / title (push, in-app)
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Nội dung có biến {{...}}
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Định dạng nội dung
    /// </summary>
    /// <remarks>Ràng buộc: <c>body_format IN ('TEXT','HTML','MARKDOWN')</c>; Mặc định DB: <c>'TEXT'</c></remarks>
    public string BodyFormat { get; set; } = "TEXT";

    /// <summary>
    /// Link mở khi bấm (có biến, VD: /hrm/leave-requests/{{id}})
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Deep link mobile app
    /// </summary>
    public string? DeepLink { get; set; }

    /// <summary>
    /// Mã template phía nhà cung cấp (Zalo ZNS, SMS brandname)
    /// </summary>
    public string? ExternalTemplateId { get; set; }

    /// <summary>
    /// Phiên bản
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public int TemplateVersion { get; set; } = 1;

    /// <summary>
    /// Đang dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Loại sự kiện thông báo do các phân hệ khai báo (hrm (qua <c>event_type_id</c>).
    /// </summary>
    public NotificationEventType EventType { get; set; } = null!;

    #endregion
}
