// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Monitor;

/// <summary>
/// Sự cố (downtime) mở/đóng tự động từ health check, phục vụ báo cáo uptime
/// </summary>
/// <remarks>Bảng <c>incidents</c> (phân hệ monitor).</remarks>
public class Incident : BaseEntity
{
    /// <summary>
    /// Service
    /// </summary>
    /// <remarks>FK → <c>monitor.services(id)</c></remarks>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Mức độ
    /// </summary>
    /// <remarks>Ràng buộc: <c>severity IN ('MINOR','MAJOR','CRITICAL')</c>; Mặc định DB: <c>'MAJOR'</c></remarks>
    public string Severity { get; set; } = "MAJOR";

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Bắt đầu sự cố
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// Khắc phục lúc
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }

    /// <summary>
    /// Nguyên nhân
    /// </summary>
    public string? RootCause { get; set; }

    /// <summary>
    /// Cách xử lý
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Người tiếp nhận
    /// </summary>
    public Guid? AcknowledgedBy { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục service trong hệ sinh thái và endpoint /health để Docker, Reverse Proxy, Uptime Monitor kiểm tra (qua <c>service_id</c>).
    /// </summary>
    public MonitoredService Service { get; set; } = null!;

    #endregion
}
