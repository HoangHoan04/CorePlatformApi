// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Prompt template dùng chung theo phân hệ (tóm tắt CV - HRM, phân tích KH - CRM, gợi ý đặt hàng - WMS)
/// </summary>
/// <remarks>Bảng <c>prompt_templates</c> (phân hệ ai); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class AiPromptTemplate : BaseEntity
{
    /// <summary>
    /// Phân hệ sử dụng
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Mã prompt service gọi (hrm.cv_summary)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Phiên bản đang dùng (ai.prompt_versions)
    /// </summary>
    public Guid? CurrentVersionId { get; set; }

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Điều hướng tới Phiên bản prompt (quản lý thay đổi, rollback, A/B test) (qua <c>current_version_id</c>).
    /// </summary>
    public AiPromptVersion? CurrentVersion { get; set; }

    /// <summary>
    /// Danh sách AiPromptVersion con (qua <c>ai.prompt_versions.template_id</c>).
    /// </summary>
    public ICollection<AiPromptVersion> AiPromptVersions { get; set; } = [];

    #endregion
}
