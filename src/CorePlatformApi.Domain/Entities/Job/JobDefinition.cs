// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Job;

/// <summary>
/// Danh mục job định kỳ toàn hệ thống (chốt công cuối ngày, cảnh báo tồn kho, dọn file rác, đồng bộ tỷ giá...)
/// </summary>
/// <remarks>Bảng <c>job.definitions</c>.</remarks>
public class JobDefinition : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã job (core.storage.cleanup_temp_files)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ sở hữu
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tên class/handler thực thi (Assembly.Namespace.JobClass)
    /// </summary>
    public string Handler { get; set; } = string.Empty;

    /// <summary>
    /// Service chạy job (hrm-worker, core-worker)
    /// </summary>
    public string? TargetService { get; set; }

    /// <summary>
    /// Lịch cron (NULL = chỉ chạy tay)
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Múi giờ tính cron
    /// </summary>
    /// <remarks>Mặc định DB: <c>'Asia/Ho_Chi_Minh'</c></remarks>
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Chạy lặp qua từng tenant
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool RunPerTenant { get; set; } = false;

    /// <summary>
    /// Tham số mặc định
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Parameters { get; set; } = "{}";

    /// <summary>
    /// Hàng đợi
    /// </summary>
    /// <remarks>Mặc định DB: <c>'default'</c></remarks>
    public string QueueName { get; set; } = "default";

    /// <summary>
    /// Thời gian chạy tối đa
    /// </summary>
    /// <remarks>Mặc định DB: <c>3600</c></remarks>
    public int TimeoutSeconds { get; set; } = 3600;

    /// <summary>
    /// Số lần retry khi lỗi
    /// </summary>
    /// <remarks>Mặc định DB: <c>3</c></remarks>
    public short MaxRetries { get; set; } = 3;

    /// <summary>
    /// Cho phép chạy chồng
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool AllowConcurrent { get; set; } = false;

    /// <summary>
    /// Gửi cảnh báo khi lỗi
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool NotifyOnFailure { get; set; } = true;

    /// <summary>
    /// Người nhận cảnh báo
    /// </summary>
    public List<Guid>? NotifyUserIds { get; set; }

    /// <summary>
    /// Đang bật
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Lần chạy gần nhất
    /// </summary>
    public DateTimeOffset? LastRunAt { get; set; }

    /// <summary>
    /// Kết quả lần gần nhất
    /// </summary>
    public string? LastStatus { get; set; }

    /// <summary>
    /// Lần chạy kế tiếp (tham khảo)
    /// </summary>
    public DateTimeOffset? NextRunAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Danh sách JobTenantOverride con (qua <c>job.tenant_overrides.job_id</c>).
    /// </summary>
    public ICollection<JobTenantOverride> JobTenantOverrides { get; set; } = [];

    /// <summary>
    /// Danh sách JobExecution con (qua <c>job.executions.job_id</c>).
    /// </summary>
    public ICollection<JobExecution> JobExecutions { get; set; } = [];

    #endregion
}
