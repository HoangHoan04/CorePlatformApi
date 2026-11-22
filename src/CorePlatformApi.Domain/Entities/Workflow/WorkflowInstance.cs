// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Một lượt chạy quy trình cho một chứng từ cụ thể
/// </summary>
/// <remarks>Bảng <c>workflow.instances</c>.</remarks>
public class WorkflowInstance : AuditableEntity, IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Quy trình
    /// </summary>
    /// <remarks>FK → <c>workflow.definitions(id)</c></remarks>
    public Guid DefinitionId { get; set; }

    /// <summary>
    /// Phiên bản đã dùng
    /// </summary>
    /// <remarks>FK → <c>workflow.definition_versions(id)</c></remarks>
    public Guid VersionId { get; set; }

    /// <summary>
    /// Loại chứng từ
    /// </summary>
    /// <remarks>FK → <c>mdm.document_types(code)</c></remarks>
    public string DocumentTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Id chứng từ bên phân hệ
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Mã chứng từ hiển thị (DXMH-2611-0012)
    /// </summary>
    public string? DocumentCode { get; set; }

    /// <summary>
    /// Tiêu đề hiển thị trong danh sách chờ duyệt
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả ngắn
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Ảnh chụp biến dùng rẽ nhánh lúc gửi duyệt (amount, leave_days...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Variables { get; set; } = "{}";

    /// <summary>
    /// Link mở chứng từ
    /// </summary>
    public string? DocumentUrl { get; set; }

    /// <summary>
    /// Người gửi duyệt
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid RequesterId { get; set; }

    /// <summary>
    /// Gửi thay cho ai (nếu có)
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid? OnBehalfOfId { get; set; }

    /// <summary>
    /// Công ty của chứng từ
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Chi nhánh
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Phòng ban
    /// </summary>
    /// <remarks>FK → <c>org.departments(id)</c></remarks>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Trạng thái tổng
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('DRAFT','PENDING','APPROVED','REJECTED','RETURNED','CANCELLED','RECALLED','ERROR')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// Tên bước hiện tại (denormalize hiển thị)
    /// </summary>
    public string? CurrentStepNames { get; set; }

    /// <summary>
    /// Độ khẩn
    /// </summary>
    /// <remarks>Ràng buộc: <c>priority IN ('LOW','NORMAL','HIGH','URGENT')</c>; Mặc định DB: <c>'NORMAL'</c></remarks>
    public string Priority { get; set; } = "NORMAL";

    /// <summary>
    /// Gửi duyệt lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Hạn hoàn thành toàn quy trình
    /// </summary>
    public DateTimeOffset? DueAt { get; set; }

    /// <summary>
    /// Kết thúc lúc
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Ý kiến cuối cùng
    /// </summary>
    public string? FinalComment { get; set; }

    /// <summary>
    /// Event phát ra khi kết thúc để phân hệ cập nhật chứng từ
    /// </summary>
    public string? CallbackEvent { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Định nghĩa quy trình duyệt cho một loại chứng từ (đơn nghỉ phép, đề xuất mua hàng, chiết khấu vượt khung (qua <c>definition_id</c>).
    /// </summary>
    public WorkflowDefinition Definition { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Phiên bản quy trình (qua <c>version_id</c>).
    /// </summary>
    public WorkflowDefinitionVersion Version { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo (qua <c>document_type_code</c>).
    /// </summary>
    public DocumentType DocumentType { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>requester_id</c>).
    /// </summary>
    public User Requester { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>on_behalf_of_id</c>).
    /// </summary>
    public User? OnBehalfOf { get; set; }

    /// <summary>
    /// Điều hướng tới Công ty con / pháp nhân thuộc tenant (qua <c>company_id</c>).
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Điều hướng tới Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty (qua <c>branch_id</c>).
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Điều hướng tới Phòng ban / bộ phận / tổ nhóm dạng cây (qua <c>department_id</c>).
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Danh sách WorkflowInstanceStep con (qua <c>workflow.instance_steps.instance_id</c>).
    /// </summary>
    public ICollection<WorkflowInstanceStep> WorkflowInstanceSteps { get; set; } = [];

    /// <summary>
    /// Danh sách WorkflowTask con (qua <c>workflow.tasks.instance_id</c>).
    /// </summary>
    public ICollection<WorkflowTask> WorkflowTasks { get; set; } = [];

    /// <summary>
    /// Danh sách WorkflowActionLog con (qua <c>workflow.action_logs.instance_id</c>).
    /// </summary>
    public ICollection<WorkflowActionLog> WorkflowActionLogs { get; set; } = [];

    #endregion
}
