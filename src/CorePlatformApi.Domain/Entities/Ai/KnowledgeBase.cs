// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ
/// </summary>
/// <remarks>Bảng <c>knowledge_bases</c> (phân hệ ai); Remove() = xóa mềm.</remarks>
[SoftDelete]
public class KnowledgeBase : BaseEntity
{
    /// <summary>
    /// Phân hệ liên quan (NULL = dùng chung)
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Mã kho
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên kho tri thức
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Model embedding (đổi model phải re-index)
    /// </summary>
    /// <remarks>FK → <c>ai.models(id)</c></remarks>
    public Guid EmbeddingModelId { get; set; }

    /// <summary>
    /// Số chiều vector
    /// </summary>
    /// <remarks>Mặc định DB: <c>1536</c></remarks>
    public int EmbeddingDimension { get; set; } = 1536;

    /// <summary>
    /// Kích thước chunk (token)
    /// </summary>
    /// <remarks>Mặc định DB: <c>800</c></remarks>
    public int ChunkSize { get; set; } = 800;

    /// <summary>
    /// Phần chồng lấn giữa các chunk
    /// </summary>
    /// <remarks>Mặc định DB: <c>100</c></remarks>
    public int ChunkOverlap { get; set; } = 100;

    /// <summary>
    /// Số chunk lấy mỗi truy vấn
    /// </summary>
    /// <remarks>Mặc định DB: <c>5</c></remarks>
    public short RetrievalTopK { get; set; } = 5;

    /// <summary>
    /// Ngưỡng tương đồng tối thiểu
    /// </summary>
    /// <remarks>Mặc định DB: <c>0.3</c></remarks>
    public decimal? MinScore { get; set; } = 0.3m;

    /// <summary>
    /// Prompt trả lời dùng cho kho này
    /// </summary>
    /// <remarks>FK → <c>ai.prompt_templates(id)</c></remarks>
    public Guid? AnswerPromptId { get; set; }

    /// <summary>
    /// Ai được hỏi đáp
    /// </summary>
    /// <remarks>Ràng buộc: <c>visibility IN ('ALL_USERS','RESTRICTED')</c>; Mặc định DB: <c>'RESTRICTED'</c></remarks>
    public string Visibility { get; set; } = "RESTRICTED";

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','INDEXING','DISABLED')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Điều hướng tới Model AI và bảng giá để ước tính chi phí (qua <c>embedding_model_id</c>).
    /// </summary>
    public AiModel EmbeddingModel { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Prompt template dùng chung theo phân hệ (tóm tắt CV - HRM, phân tích KH - CRM, gợi ý đặt hàng - WMS) (qua <c>answer_prompt_id</c>).
    /// </summary>
    public AiPromptTemplate? AnswerPrompt { get; set; }

    /// <summary>
    /// Danh sách KnowledgeBaseAccess con (qua <c>ai.knowledge_base_access.knowledge_base_id</c>).
    /// </summary>
    public ICollection<KnowledgeBaseAccess> KnowledgeBaseAccesses { get; set; } = [];

    /// <summary>
    /// Danh sách KbDocument con (qua <c>ai.kb_documents.knowledge_base_id</c>).
    /// </summary>
    public ICollection<KbDocument> KbDocuments { get; set; } = [];

    /// <summary>
    /// Danh sách KbChunk con (qua <c>ai.kb_chunks.knowledge_base_id</c>).
    /// </summary>
    public ICollection<KbChunk> KbChunks { get; set; } = [];

    #endregion
}
