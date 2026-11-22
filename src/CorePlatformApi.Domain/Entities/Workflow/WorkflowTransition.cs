// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Cạnh nối giữa các bước kèm điều kiện rẽ nhánh (VD: amount &lt; 10000000 → Trưởng BP; &gt;= 50000000 → Giám đốc)
/// </summary>
/// <remarks>Bảng <c>workflow.transitions</c>.</remarks>
public class WorkflowTransition : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Phiên bản
    /// </summary>
    /// <remarks>FK → <c>workflow.definition_versions(id)</c></remarks>
    public Guid VersionId { get; set; }

    /// <summary>
    /// Bước nguồn
    /// </summary>
    /// <remarks>FK → <c>workflow.steps(id)</c></remarks>
    public Guid FromStepId { get; set; }

    /// <summary>
    /// Bước đích
    /// </summary>
    /// <remarks>FK → <c>workflow.steps(id)</c></remarks>
    public Guid ToStepId { get; set; }

    /// <summary>
    /// Kết quả bước nguồn kích hoạt cạnh này
    /// </summary>
    /// <remarks>Ràng buộc: <c>trigger_outcome IN ('APPROVED','REJECTED','RETURNED','ANY','TIMEOUT')</c>; Mặc định DB: <c>'APPROVED'</c></remarks>
    public string TriggerOutcome { get; set; } = "APPROVED";

    /// <summary>
    /// Nhãn cạnh (Dưới 10 triệu)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Điều kiện dạng cây JSON, VD: {"all":[{"field":"amount","op":"lt","value":10000000}]}
    /// </summary>
    public string? ConditionJson { get; set; }

    /// <summary>
    /// Điều kiện dạng biểu thức (thay thế condition_json), VD: amount &gt;= 10000000 &amp;&amp; amount &lt; 50000000
    /// </summary>
    public string? ConditionExpr { get; set; }

    /// <summary>
    /// Thứ tự xét điều kiện (nhỏ xét trước)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Nhánh mặc định khi không điều kiện nào khớp
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsDefault { get; set; } = false;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Phiên bản quy trình (qua <c>version_id</c>).
    /// </summary>
    public WorkflowDefinitionVersion Version { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc (qua <c>from_step_id</c>).
    /// </summary>
    public WorkflowStep FromStep { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc (qua <c>to_step_id</c>).
    /// </summary>
    public WorkflowStep ToStep { get; set; } = null!;

    #endregion
}
