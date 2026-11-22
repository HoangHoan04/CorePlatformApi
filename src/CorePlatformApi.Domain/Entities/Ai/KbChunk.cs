// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Chunk văn bản + vector embedding (pgvector) + full-text để hybrid search
/// </summary>
/// <remarks>Bảng <c>ai.kb_chunks</c>.</remarks>
public class KbChunk : IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tài liệu
    /// </summary>
    /// <remarks>FK → <c>ai.kb_documents(id)</c></remarks>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Kho (denormalize để lọc nhanh)
    /// </summary>
    /// <remarks>FK → <c>ai.knowledge_bases(id)</c></remarks>
    public Guid KnowledgeBaseId { get; set; }

    /// <summary>
    /// Tenant
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Thứ tự chunk trong tài liệu
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Nội dung chunk
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn tiêu đề (Chương 2 &gt; Điều 5) để trích dẫn
    /// </summary>
    public string? HeadingPath { get; set; }

    /// <summary>
    /// Số trang (PDF)
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Số token
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    /// Metadata lọc (department, tags...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Vector embedding; đổi số chiều theo model (VD: 768 Gemini, 3072 text-embedding-3-large dùng halfvec)
    /// </summary>
    /// <remarks>Kiểu DB <c>vector(1536)</c> (pgvector)</remarks>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Tạo lúc
    /// </summary>
    /// <remarks>Mặc định DB: <c>now()</c></remarks>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài liệu nguồn trong kho tri thức (qua <c>document_id</c>).
    /// </summary>
    public KbDocument Document { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ (qua <c>knowledge_base_id</c>).
    /// </summary>
    public KnowledgeBase KnowledgeBase { get; set; } = null!;

    #endregion
}
