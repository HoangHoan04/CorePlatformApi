// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Lượt thực thi của từng bước trong instance (một bước có thể lặp lại khi bị trả về)
/// </summary>
/// <remarks>Bảng <c>workflow.instance_steps</c>.</remarks>
public class WorkflowInstanceStep
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Instance
    /// </summary>
    /// <remarks>FK → <c>workflow.instances(id)</c></remarks>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// Bước định nghĩa
    /// </summary>
    /// <remarks>FK → <c>workflow.steps(id)</c></remarks>
    public Guid StepId { get; set; }

    /// <summary>
    /// Lần chạy thứ mấy của bước (tăng khi bị trả lại)
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short Iteration { get; set; } = 1;

    /// <summary>
    /// Trạng thái bước
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('WAITING','IN_PROGRESS','APPROVED','REJECTED','RETURNED','SKIPPED','TIMEOUT','CANCELLED')</c>; Mặc định DB: <c>'IN_PROGRESS'</c></remarks>
    public string Status { get; set; } = "IN_PROGRESS";

    /// <summary>
    /// Kết quả để chọn transition
    /// </summary>
    public string? Outcome { get; set; }

    /// <summary>
    /// Bắt đầu
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Hạn xử lý (đã tính theo lịch làm việc)
    /// </summary>
    public DateTimeOffset? DueAt { get; set; }

    /// <summary>
    /// Hoàn thành
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Đã leo thang lúc
    /// </summary>
    public DateTimeOffset? EscalatedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Một lượt chạy quy trình cho một chứng từ cụ thể (qua <c>instance_id</c>).
    /// </summary>
    public WorkflowInstance Instance { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc (qua <c>step_id</c>).
    /// </summary>
    public WorkflowStep Step { get; set; } = null!;

    /// <summary>
    /// Danh sách WorkflowTask con (qua <c>workflow.tasks.instance_step_id</c>).
    /// </summary>
    public ICollection<WorkflowTask> WorkflowTasks { get; set; } = [];

    #endregion
}
