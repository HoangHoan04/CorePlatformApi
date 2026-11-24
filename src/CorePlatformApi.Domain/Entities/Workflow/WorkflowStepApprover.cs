// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Người duyệt của một bước: chỉ định cụ thể, theo vai trò, quản lý trực tiếp, trưởng phòng, cấp quản lý thứ N...
/// </summary>
/// <remarks>Bảng <c>step_approvers</c> (phân hệ workflow).</remarks>
public class WorkflowStepApprover : BaseEntity
{
    /// <summary>
    /// Bước
    /// </summary>
    /// <remarks>FK → <c>workflow.steps(id)</c></remarks>
    public Guid StepId { get; set; }

    /// <summary>
    /// Cách xác định người duyệt
    /// </summary>
    /// <remarks>Ràng buộc: <c>approver_type IN ('USER','ROLE','DIRECT_MANAGER','MANAGER_LEVEL','DEPARTMENT_MANAGER','BRANCH_MANAGER','COMPANY_DIRECTOR','REQUESTER','FIELD_VALUE','EXPRESSION')</c></remarks>
    public string ApproverType { get; set; } = string.Empty;

    /// <summary>
    /// Id user / role / phòng ban tương ứng
    /// </summary>
    public Guid? ApproverRefId { get; set; }

    /// <summary>
    /// Cấp quản lý thứ N (MANAGER_LEVEL)
    /// </summary>
    public short? ManagerLevel { get; set; }

    /// <summary>
    /// Lấy người duyệt từ trường của chứng từ (FIELD_VALUE)
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Biểu thức tùy biến (EXPRESSION)
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// ROLE: lọc người có vai trò trong đơn vị của người tạo hay của chứng từ
    /// </summary>
    /// <remarks>Ràng buộc: <c>scope_by IN ('REQUESTER','DOCUMENT')</c></remarks>
    public string? ScopeBy { get; set; }

    /// <summary>
    /// Thứ tự khi duyệt SEQUENTIAL
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int SortOrder { get; set; } = 0;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc (qua <c>step_id</c>).
    /// </summary>
    public WorkflowStep Step { get; set; } = null!;

    #endregion
}
