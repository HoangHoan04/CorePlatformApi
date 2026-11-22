// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Mdm;

/// <summary>
/// Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo
/// </summary>
/// <remarks>Bảng <c>mdm.document_types</c>.</remarks>
public class DocumentType : AuditableEntity
{
    /// <summary>
    /// Mã loại chứng từ (HRM_EMPLOYEE, HRM_LEAVE_REQUEST, WMS_GOODS_RECEIPT, CRM_QUOTATION)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ sở hữu
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Tên loại chứng từ
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bảng dữ liệu gốc bên phân hệ (tham khảo)
    /// </summary>
    public string? EntityTable { get; set; }

    /// <summary>
    /// Có thể gắn quy trình duyệt
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool SupportsWorkflow { get; set; } = false;

    /// <summary>
    /// Có sinh mã tự động
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool SupportsNumbering { get; set; } = true;

    /// <summary>
    /// Cho phép đính kèm file
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool SupportsAttachment { get; set; } = true;

    /// <summary>
    /// Khai báo biến dùng trong điều kiện rẽ nhánh, VD: [{"name":"amount","type":"number","label":"Tổng tiền"}]
    /// </summary>
    public string? WorkflowVariables { get; set; }

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module Module { get; set; } = null!;

    #endregion
}
