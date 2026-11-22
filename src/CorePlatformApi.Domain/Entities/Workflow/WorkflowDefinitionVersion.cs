// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Phiên bản quy trình. Instance đang chạy giữ nguyên phiên bản cũ khi admin sửa quy trình
/// </summary>
/// <remarks>Bảng <c>workflow.definition_versions</c>.</remarks>
public class WorkflowDefinitionVersion : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Quy trình
    /// </summary>
    /// <remarks>FK → <c>workflow.definitions(id)</c></remarks>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// Số phiên bản
    /// </summary>
    public int VersionNo { get; set; }

    /// <summary>
    /// Nháp, đã phát hành, lưu trữ
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('DRAFT','PUBLISHED','ARCHIVED')</c>; Mặc định DB: <c>'DRAFT'</c></remarks>
    public string Status { get; set; } = "DRAFT";

    /// <summary>
    /// Dữ liệu sơ đồ trên designer (vị trí node, cạnh) để vẽ lại UI
    /// </summary>
    public string? DesignerJson { get; set; }

    /// <summary>
    /// Ghi chú thay đổi
    /// </summary>
    public string? ChangeNote { get; set; }

    /// <summary>
    /// Phát hành lúc
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Người phát hành
    /// </summary>
    public Guid? PublishedBy { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Định nghĩa quy trình duyệt cho một loại chứng từ (đơn nghỉ phép, đề xuất mua hàng, chiết khấu vượt khung (qua <c>definition_id</c>).
    /// </summary>
    public WorkflowDefinition Definition { get; set; } = null!;

    /// <summary>
    /// Danh sách WorkflowStep con (qua <c>workflow.steps.version_id</c>).
    /// </summary>
    public ICollection<WorkflowStep> WorkflowSteps { get; set; } = [];

    /// <summary>
    /// Danh sách WorkflowTransition con (qua <c>workflow.transitions.version_id</c>).
    /// </summary>
    public ICollection<WorkflowTransition> WorkflowTransitions { get; set; } = [];

    #endregion
}
