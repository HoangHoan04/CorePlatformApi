// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Job;

/// <summary>
/// Lịch sử từng lần chạy job
/// </summary>
/// <remarks>Bảng <c>job.executions</c>.</remarks>
public class JobExecution : IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Job
    /// </summary>
    /// <remarks>FK → <c>job.definitions(id)</c></remarks>
    public Guid JobId { get; set; }

    /// <summary>
    /// Tenant (job chạy theo tenant)
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Nguồn kích hoạt
    /// </summary>
    /// <remarks>Ràng buộc: <c>trigger_type IN ('SCHEDULED','MANUAL','RETRY','EVENT','API')</c>; Mặc định DB: <c>'SCHEDULED'</c></remarks>
    public string TriggerType { get; set; } = "SCHEDULED";

    /// <summary>
    /// User chạy tay
    /// </summary>
    public Guid? TriggeredBy { get; set; }

    /// <summary>
    /// Id job bên Hangfire/Quartz
    /// </summary>
    public string? EngineJobId { get; set; }

    /// <summary>
    /// Máy/container thực thi
    /// </summary>
    public string? WorkerNode { get; set; }

    /// <summary>
    /// Tham số thực tế
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('QUEUED','RUNNING','SUCCEEDED','FAILED','CANCELLED','TIMEOUT','SKIPPED')</c>; Mặc định DB: <c>'RUNNING'</c></remarks>
    public string Status { get; set; } = "RUNNING";

    /// <summary>
    /// Tiến độ %
    /// </summary>
    public short? ProgressPercent { get; set; }

    /// <summary>
    /// Số bản ghi đã xử lý
    /// </summary>
    public int? ProcessedCount { get; set; }

    /// <summary>
    /// Số bản ghi lỗi
    /// </summary>
    public int? FailedCount { get; set; }

    /// <summary>
    /// Tóm tắt kết quả
    /// </summary>
    public string? ResultSummary { get; set; }

    /// <summary>
    /// Lỗi
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace
    /// </summary>
    public string? ErrorStack { get; set; }

    /// <summary>
    /// Lần thử
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short AttemptNo { get; set; } = 1;

    /// <summary>
    /// Bắt đầu
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Kết thúc
    /// </summary>
    public DateTimeOffset? FinishedAt { get; set; }

    /// <summary>
    /// Thời gian chạy
    /// </summary>
    public int? DurationMs { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục job định kỳ toàn hệ thống (chốt công cuối ngày, cảnh báo tồn kho, dọn file rác, đồng bộ tỷ giá (qua <c>job_id</c>).
    /// </summary>
    public JobDefinition Job { get; set; } = null!;

    #endregion
}
