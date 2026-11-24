// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Storage;
using System.Net;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Nhiệm vụ duyệt giao cho từng người (danh sách "Việc chờ tôi duyệt")
/// </summary>
/// <remarks>Bảng <c>tasks</c> (phân hệ workflow).</remarks>
public class WorkflowTask : BaseEntity
{
    /// <summary>
    /// Instance
    /// </summary>
    /// <remarks>FK → <c>workflow.instances(id)</c></remarks>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// Lượt bước
    /// </summary>
    /// <remarks>FK → <c>workflow.instance_steps(id)</c></remarks>
    public Guid InstanceStepId { get; set; }

    /// <summary>
    /// Người đang phải xử lý
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid AssigneeId { get; set; }

    /// <summary>
    /// Người được giao gốc (khác assignee khi ủy quyền/chuyển)
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid? OriginalAssigneeId { get; set; }

    /// <summary>
    /// Ủy quyền đã áp dụng (workflow.delegations)
    /// </summary>
    public Guid? DelegationId { get; set; }

    /// <summary>
    /// Duyệt, xin ý kiến, chỉ xem/biết
    /// </summary>
    /// <remarks>Ràng buộc: <c>task_type IN ('APPROVE','CONSULT','ACKNOWLEDGE')</c>; Mặc định DB: <c>'APPROVE'</c></remarks>
    public string TaskType { get; set; } = "APPROVE";

    /// <summary>
    /// Thứ tự khi duyệt tuần tự
    /// </summary>
    /// <remarks>Mặc định DB: <c>1</c></remarks>
    public short SequenceNo { get; set; } = 1;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('WAITING','PENDING','APPROVED','REJECTED','RETURNED','DELEGATED','REASSIGNED','CANCELLED','EXPIRED','SKIPPED')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// Ý kiến khi xử lý
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Ảnh chữ ký
    /// </summary>
    /// <remarks>FK → <c>storage.files(id)</c></remarks>
    public Guid? SignatureFileId { get; set; }

    /// <summary>
    /// Hạn xử lý
    /// </summary>
    public DateTimeOffset? DueAt { get; set; }

    /// <summary>
    /// Số lần đã nhắc
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public short ReminderCount { get; set; } = 0;

    /// <summary>
    /// Nhắc gần nhất
    /// </summary>
    public DateTimeOffset? LastRemindedAt { get; set; }

    /// <summary>
    /// Đã xem lúc
    /// </summary>
    public DateTimeOffset? ReadAt { get; set; }

    /// <summary>
    /// Xử lý lúc
    /// </summary>
    public DateTimeOffset? ActedAt { get; set; }

    /// <summary>
    /// IP khi xử lý
    /// </summary>
    public IPAddress? ActedIp { get; set; }

    /// <summary>
    /// Kênh xử lý
    /// </summary>
    /// <remarks>Ràng buộc: <c>acted_via IN ('WEB','MOBILE','EMAIL','API','SYSTEM')</c></remarks>
    public string? ActedVia { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Một lượt chạy quy trình cho một chứng từ cụ thể (qua <c>instance_id</c>).
    /// </summary>
    public WorkflowInstance Instance { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Lượt thực thi của từng bước trong instance (một bước có thể lặp lại khi bị trả về) (qua <c>instance_step_id</c>).
    /// </summary>
    public WorkflowInstanceStep InstanceStep { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>assignee_id</c>).
    /// </summary>
    public User Assignee { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>original_assignee_id</c>).
    /// </summary>
    public User? OriginalAssignee { get; set; }

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>signature_file_id</c>).
    /// </summary>
    public StorageFile? SignatureFile { get; set; }

    /// <summary>
    /// Điều hướng tới Ủy quyền duyệt khi vắng mặt (đi công tác, nghỉ phép) (qua <c>delegation_id</c>).
    /// </summary>
    public WorkflowDelegation? Delegation { get; set; }

    #endregion
}
