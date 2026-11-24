// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc
/// </summary>
/// <remarks>Bảng <c>steps</c> (phân hệ workflow).</remarks>
public class WorkflowStep : BaseEntity
{
    /// <summary>
    /// Phiên bản
    /// </summary>
    /// <remarks>FK → <c>workflow.definition_versions(id)</c></remarks>
    public Guid VersionId { get; set; }

    /// <summary>
    /// Mã bước (trong phiên bản)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên bước (Trưởng bộ phận duyệt)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại node
    /// </summary>
    /// <remarks>Ràng buộc: <c>step_type IN ('START','APPROVAL','CONDITION','PARALLEL_SPLIT','PARALLEL_JOIN','NOTIFY','SYSTEM','END')</c></remarks>
    public string StepType { get; set; } = string.Empty;

    /// <summary>
    /// Cách duyệt khi nhiều người: 1 người bất kỳ, tất cả, lần lượt, đủ tỷ lệ
    /// </summary>
    /// <remarks>Ràng buộc: <c>approval_mode IN ('ANY','ALL','SEQUENTIAL','QUORUM')</c></remarks>
    public string? ApprovalMode { get; set; }

    /// <summary>
    /// Tỷ lệ % cần đồng ý (QUORUM)
    /// </summary>
    /// <remarks>Ràng buộc: <c>quorum_percent BETWEEN 1 AND 100</c></remarks>
    public short? QuorumPercent { get; set; }

    /// <summary>
    /// Hạn xử lý (giờ)
    /// </summary>
    public decimal? SlaHours { get; set; }

    /// <summary>
    /// Tính hạn theo giờ làm việc (mdm.work_calendars), bỏ qua lễ/CN
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool SlaUseWorkingCalendar { get; set; } = true;

    /// <summary>
    /// Nhắc trước hạn N giờ
    /// </summary>
    public decimal? RemindBeforeHours { get; set; }

    /// <summary>
    /// Nhắc lặp lại mỗi N giờ khi quá hạn
    /// </summary>
    public decimal? RemindIntervalHours { get; set; }

    /// <summary>
    /// Số lần nhắc tối đa
    /// </summary>
    /// <remarks>Mặc định DB: <c>3</c></remarks>
    public short? MaxReminders { get; set; } = 3;

    /// <summary>
    /// Hành động khi quá hạn
    /// </summary>
    /// <remarks>Ràng buộc: <c>timeout_action IN ('NONE','REMIND','ESCALATE','AUTO_APPROVE','AUTO_REJECT')</c>; Mặc định DB: <c>'REMIND'</c></remarks>
    public string TimeoutAction { get; set; } = "REMIND";

    /// <summary>
    /// Loại người nhận khi leo thang (giống approver_type)
    /// </summary>
    public string? EscalateToType { get; set; }

    /// <summary>
    /// Id người/vai trò nhận leo thang
    /// </summary>
    public Guid? EscalateToRef { get; set; }

    /// <summary>
    /// Cho phép từ chối
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool AllowReject { get; set; } = true;

    /// <summary>
    /// Cho phép trả lại bước trước/người tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool AllowReturn { get; set; } = true;

    /// <summary>
    /// Cho phép chuyển/ủy quyền
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool AllowDelegate { get; set; } = true;

    /// <summary>
    /// Cho phép xin ý kiến thêm người
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool AllowAddApprover { get; set; } = false;

    /// <summary>
    /// Bắt buộc ghi lý do khi từ chối
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool RequireCommentOnReject { get; set; } = true;

    /// <summary>
    /// Yêu cầu chữ ký (ảnh/số)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool RequireSignature { get; set; } = false;

    /// <summary>
    /// Trường chứng từ người duyệt bước này được sửa
    /// </summary>
    public List<string>? EditableFields { get; set; }

    /// <summary>
    /// Hành động hệ thống khi vào bước (gọi webhook, phát event)
    /// </summary>
    public string? OnEnterActions { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Phiên bản quy trình (qua <c>version_id</c>).
    /// </summary>
    public WorkflowDefinitionVersion WorkflowDefinitionVersion { get; set; } = null!;

    /// <summary>
    /// Danh sách WorkflowStepApprover con (qua <c>workflow.step_approvers.step_id</c>).
    /// </summary>
    public ICollection<WorkflowStepApprover> WorkflowStepApprovers { get; set; } = [];

    #endregion
}
