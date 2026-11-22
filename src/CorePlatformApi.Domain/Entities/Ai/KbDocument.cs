// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Storage;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Tài liệu nguồn trong kho tri thức
/// </summary>
/// <remarks>Bảng <c>ai.kb_documents</c>.</remarks>
public class KbDocument : SoftDeletableEntity, IMustHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Kho tri thức
    /// </summary>
    /// <remarks>FK → <c>ai.knowledge_bases(id)</c></remarks>
    public Guid KnowledgeBaseId { get; set; }

    /// <summary>
    /// Tenant (denormalize)
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Nguồn: file, URL, văn bản nhập tay, bản ghi nghiệp vụ
    /// </summary>
    /// <remarks>Ràng buộc: <c>source_type IN ('FILE','URL','TEXT','ENTITY')</c></remarks>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// File gốc
    /// </summary>
    /// <remarks>FK → <c>storage.files(id)</c></remarks>
    public Guid? FileId { get; set; }

    /// <summary>
    /// URL nguồn
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Nội dung text (TEXT) hoặc text đã trích xuất
    /// </summary>
    public string? ContentText { get; set; }

    /// <summary>
    /// Hash nội dung để bỏ qua re-index khi không đổi
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Ngôn ngữ
    /// </summary>
    /// <remarks>Mặc định DB: <c>'vi'</c></remarks>
    public string? Language { get; set; } = "vi";

    /// <summary>
    /// Thẻ phân loại
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Hiệu lực từ (VD: nội quy bản 2026)
    /// </summary>
    public DateOnly? EffectiveFrom { get; set; }

    /// <summary>
    /// Hết hiệu lực → loại khỏi kết quả tìm kiếm
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Số chunk đã tạo
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int ChunkCount { get; set; } = 0;

    /// <summary>
    /// Tổng token
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    /// Trạng thái index
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('PENDING','PROCESSING','INDEXED','FAILED','DISABLED')</c>; Mặc định DB: <c>'PENDING'</c></remarks>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// Lỗi index
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Index xong lúc
    /// </summary>
    public DateTimeOffset? IndexedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ (qua <c>knowledge_base_id</c>).
    /// </summary>
    public KnowledgeBase KnowledgeBase { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Metadata file đã upload (qua <c>file_id</c>).
    /// </summary>
    public StorageFile? File { get; set; }

    /// <summary>
    /// Danh sách KbChunk con (qua <c>ai.kb_chunks.document_id</c>).
    /// </summary>
    public ICollection<KbChunk> KbChunks { get; set; } = [];

    #endregion
}
