// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Lịch sử thao tác trên instance (timeline duyệt)
/// </summary>
/// <remarks>Bảng <c>action_logs</c> (phân hệ workflow).</remarks>
public class WorkflowActionLog : BaseEntity
{
    /// <summary>
    /// Instance
    /// </summary>
    /// <remarks>FK → <c>workflow.instances(id)</c></remarks>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// Nhiệm vụ liên quan
    /// </summary>
    /// <remarks>FK → <c>workflow.tasks(id)</c></remarks>
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Lượt bước
    /// </summary>
    /// <remarks>FK → <c>workflow.instance_steps(id)</c></remarks>
    public Guid? InstanceStepId { get; set; }

    /// <summary>
    /// Hành động
    /// </summary>
    /// <remarks>Ràng buộc: <c>action IN ('SUBMIT','APPROVE','REJECT','RETURN','RECALL','CANCEL','DELEGATE','REASSIGN','CONSULT','COMMENT','REMIND','ESCALATE','AUTO_APPROVE','AUTO_REJECT','SKIP','RESUBMIT')</c></remarks>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Người thao tác (NULL = hệ thống)
    /// </summary>
    public Guid? ActorId { get; set; }

    /// <summary>
    /// Thao tác thay cho ai (ủy quyền)
    /// </summary>
    public Guid? OnBehalfOfId { get; set; }

    /// <summary>
    /// Người nhận (khi chuyển/ủy quyền/xin ý kiến)
    /// </summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>
    /// Bước trước
    /// </summary>
    public string? FromStepName { get; set; }

    /// <summary>
    /// Bước sau
    /// </summary>
    public string? ToStepName { get; set; }

    /// <summary>
    /// Ý kiến
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// File đính kèm (storage.files)
    /// </summary>
    public List<Guid>? Attachments { get; set; }

    /// <summary>
    /// Dữ liệu chứng từ được người duyệt sửa
    /// </summary>
    public string? DataChanges { get; set; }

    /// <summary>
    /// IP
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Một lượt chạy quy trình cho một chứng từ cụ thể (qua <c>instance_id</c>).
    /// </summary>
    public WorkflowInstance Instance { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Nhiệm vụ duyệt giao cho từng người (danh sách "Việc chờ tôi duyệt") (qua <c>task_id</c>).
    /// </summary>
    public WorkflowTask? Task { get; set; }

    /// <summary>
    /// Điều hướng tới Lượt thực thi của từng bước trong instance (một bước có thể lặp lại khi bị trả về) (qua <c>instance_step_id</c>).
    /// </summary>
    public WorkflowInstanceStep? InstanceStep { get; set; }

    #endregion
}
