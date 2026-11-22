// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Monitor;

/// <summary>
/// Danh mục service trong hệ sinh thái và endpoint /health để Docker, Reverse Proxy, Uptime Monitor kiểm tra
/// </summary>
/// <remarks>Bảng <c>monitor.services</c>.</remarks>
public class MonitoredService : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã service (auth-api, hrm-api, core-worker, postgres, redis, rabbitmq, minio)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Loại
    /// </summary>
    /// <remarks>Ràng buộc: <c>service_type IN ('API','WORKER','WEB','DATABASE','CACHE','QUEUE','STORAGE','EXTERNAL')</c>; Mặc định DB: <c>'API'</c></remarks>
    public string ServiceType { get; set; } = "API";

    /// <summary>
    /// Môi trường
    /// </summary>
    /// <remarks>Ràng buộc: <c>environment IN ('DEVELOPMENT','STAGING','PRODUCTION')</c>; Mặc định DB: <c>'PRODUCTION'</c></remarks>
    public string Environment { get; set; } = "PRODUCTION";

    /// <summary>
    /// URL nội bộ
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// URL health (/health/ready)
    /// </summary>
    public string? HealthUrl { get; set; }

    /// <summary>
    /// URL liveness (/health/live)
    /// </summary>
    public string? LivenessUrl { get; set; }

    /// <summary>
    /// Chu kỳ kiểm tra
    /// </summary>
    /// <remarks>Mặc định DB: <c>30</c></remarks>
    public int CheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Timeout
    /// </summary>
    /// <remarks>Mặc định DB: <c>5000</c></remarks>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Cảnh báo sau N lần lỗi liên tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>3</c></remarks>
    public short AlertAfterFailures { get; set; } = 3;

    /// <summary>
    /// Kênh cảnh báo (email, Telegram, Slack webhook)
    /// </summary>
    public string? AlertChannels { get; set; }

    /// <summary>
    /// Phiên bản đang chạy (đọc từ /health)
    /// </summary>
    public string? CurrentVersion { get; set; }

    /// <summary>
    /// Trạng thái hiện tại
    /// </summary>
    /// <remarks>Ràng buộc: <c>current_status IN ('HEALTHY','DEGRADED','UNHEALTHY','UNKNOWN','MAINTENANCE')</c>; Mặc định DB: <c>'UNKNOWN'</c></remarks>
    public string CurrentStatus { get; set; } = "UNKNOWN";

    /// <summary>
    /// Số lần lỗi liên tiếp
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int ConsecutiveFailures { get; set; } = 0;

    /// <summary>
    /// Kiểm tra gần nhất
    /// </summary>
    public DateTimeOffset? LastCheckedAt { get; set; }

    /// <summary>
    /// Đổi trạng thái gần nhất
    /// </summary>
    public DateTimeOffset? LastStatusChangeAt { get; set; }

    /// <summary>
    /// Đang giám sát
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    #endregion
}
