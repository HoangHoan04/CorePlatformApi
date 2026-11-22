// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Workflow;

/// <summary>
/// Ủy quyền duyệt khi vắng mặt (đi công tác, nghỉ phép)
/// </summary>
/// <remarks>Bảng <c>workflow.delegations</c>.</remarks>
public class WorkflowDelegation : AuditableEntity, IMustHaveTenant
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
    /// Người ủy quyền
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid DelegatorId { get; set; }

    /// <summary>
    /// Người được ủy quyền
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid DelegateId { get; set; }

    /// <summary>
    /// Phạm vi ủy quyền
    /// </summary>
    /// <remarks>Ràng buộc: <c>scope IN ('ALL','MODULE','DOCUMENT_TYPE','DEFINITION')</c>; Mặc định DB: <c>'ALL'</c></remarks>
    public string Scope { get; set; } = "ALL";

    /// <summary>
    /// Phân hệ (scope = MODULE)
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Loại chứng từ (scope = DOCUMENT_TYPE)
    /// </summary>
    /// <remarks>FK → <c>mdm.document_types(code)</c></remarks>
    public string? DocumentTypeCode { get; set; }

    /// <summary>
    /// Quy trình (scope = DEFINITION)
    /// </summary>
    /// <remarks>FK → <c>workflow.definitions(id)</c></remarks>
    public Guid? DefinitionId { get; set; }

    /// <summary>
    /// Chỉ ủy quyền chứng từ có giá trị ≤ mức này
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Bắt đầu
    /// </summary>
    public DateTimeOffset StartAt { get; set; }

    /// <summary>
    /// Kết thúc
    /// </summary>
    public DateTimeOffset EndAt { get; set; }

    /// <summary>
    /// Lý do
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Chuyển cả các task đang chờ cho người được ủy quyền
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IncludePending { get; set; } = true;

    /// <summary>
    /// Vẫn gửi thông báo cho người ủy quyền
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool NotifyDelegator { get; set; } = true;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','REVOKED','EXPIRED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Thu hồi lúc
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>delegator_id</c>).
    /// </summary>
    public User Delegator { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>delegate_id</c>).
    /// </summary>
    public User Delegate { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo (qua <c>document_type_code</c>).
    /// </summary>
    public DocumentType? DocumentType { get; set; }

    /// <summary>
    /// Điều hướng tới Định nghĩa quy trình duyệt cho một loại chứng từ (đơn nghỉ phép, đề xuất mua hàng, chiết khấu vượt khung (qua <c>definition_id</c>).
    /// </summary>
    public WorkflowDefinition? Definition { get; set; }

    #endregion
}
