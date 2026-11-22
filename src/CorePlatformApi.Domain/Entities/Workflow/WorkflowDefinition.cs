// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Định nghĩa quy trình duyệt cho một loại chứng từ (đơn nghỉ phép, đề xuất mua hàng, chiết khấu vượt khung...)
/// </summary>
/// <remarks>Bảng <c>workflow.definitions</c>.</remarks>
public class WorkflowDefinition : SoftDeletableEntity, IMustHaveTenant
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
    /// Loại chứng từ áp dụng
    /// </summary>
    /// <remarks>FK → <c>mdm.document_types(code)</c></remarks>
    public string DocumentTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// Mã quy trình
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên quy trình
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Áp dụng riêng công ty (NULL = toàn tenant)
    /// </summary>
    /// <remarks>FK → <c>org.companies(id)</c></remarks>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Áp dụng riêng chi nhánh
    /// </summary>
    /// <remarks>FK → <c>org.branches(id)</c></remarks>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Áp dụng riêng phòng ban
    /// </summary>
    /// <remarks>FK → <c>org.departments(id)</c></remarks>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Điều kiện chọn quy trình khi có nhiều quy trình cùng loại chứng từ
    /// </summary>
    public string? ApplicableCondition { get; set; }

    /// <summary>
    /// Ưu tiên khi nhiều quy trình cùng khớp (số lớn ưu tiên)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Phiên bản đang áp dụng (workflow.definition_versions)
    /// </summary>
    public Guid? CurrentVersionId { get; set; }

    /// <summary>
    /// Người tạo được thu hồi khi chưa có ai duyệt
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool AllowRecall { get; set; } = true;

    /// <summary>
    /// Cho sửa chứng từ khi đang chờ duyệt
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool AllowEditWhilePending { get; set; } = false;

    /// <summary>
    /// Tự bỏ qua bước nếu người duyệt chính là người tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool AutoApproveIfRequesterIsApprover { get; set; } = true;

    /// <summary>
    /// Một người xuất hiện ở nhiều bước chỉ cần duyệt một lần
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool SkipDuplicateApprover { get; set; } = true;

    /// <summary>
    /// Đang áp dụng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo (qua <c>document_type_code</c>).
    /// </summary>
    public DocumentType DocumentType { get; set; } = null!;

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
    /// Điều hướng tới Phiên bản quy trình (qua <c>current_version_id</c>).
    /// </summary>
    public WorkflowDefinitionVersion? CurrentVersion { get; set; }

    /// <summary>
    /// Danh sách WorkflowDefinitionVersion con (qua <c>workflow.definition_versions.definition_id</c>).
    /// </summary>
    public ICollection<WorkflowDefinitionVersion> WorkflowDefinitionVersions { get; set; } = [];

    #endregion
}
