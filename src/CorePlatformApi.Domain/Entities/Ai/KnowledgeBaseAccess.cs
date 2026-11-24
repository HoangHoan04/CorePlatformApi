// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Phân quyền truy cập kho tri thức theo vai trò/phòng ban/chi nhánh/user
/// </summary>
/// <remarks>Bảng <c>knowledge_base_access</c> (phân hệ ai).</remarks>
public class KnowledgeBaseAccess : BaseEntity
{
    /// <summary>
    /// Kho tri thức
    /// </summary>
    /// <remarks>FK → <c>ai.knowledge_bases(id)</c></remarks>
    public Guid KnowledgeBaseId { get; set; }

    /// <summary>
    /// Loại đối tượng
    /// </summary>
    /// <remarks>Ràng buộc: <c>principal_type IN ('ROLE','DEPARTMENT','BRANCH','COMPANY','USER')</c></remarks>
    public string PrincipalType { get; set; } = string.Empty;

    /// <summary>
    /// Id đối tượng
    /// </summary>
    public Guid PrincipalId { get; set; }

    /// <summary>
    /// Được quản lý tài liệu
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool CanManage { get; set; } = false;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ (qua <c>knowledge_base_id</c>).
    /// </summary>
    public KnowledgeBase KnowledgeBase { get; set; } = null!;

    #endregion
}
