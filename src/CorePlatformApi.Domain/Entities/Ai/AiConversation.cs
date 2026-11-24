// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Hội thoại chatbot nội bộ
/// </summary>
/// <remarks>Bảng <c>conversations</c> (phân hệ ai).</remarks>
public class AiConversation : BaseEntity
{
    /// <summary>
    /// Người hỏi
    /// </summary>
    /// <remarks>FK → <c>iam.users(id)</c></remarks>
    public Guid UserId { get; set; }

    /// <summary>
    /// Kho tri thức (NULL = chat tự do)
    /// </summary>
    /// <remarks>FK → <c>ai.knowledge_bases(id)</c></remarks>
    public Guid? KnowledgeBaseId { get; set; }

    /// <summary>
    /// Phân hệ mở chatbot
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Tiêu đề (tự sinh từ câu đầu)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Lưu trữ
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Tin nhắn cuối
    /// </summary>
    public DateTimeOffset? LastMessageAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web &amp; Mobile) (qua <c>user_id</c>).
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ (qua <c>knowledge_base_id</c>).
    /// </summary>
    public KnowledgeBase? KnowledgeBase { get; set; }

    /// <summary>
    /// Danh sách AiMessage con (qua <c>ai.messages.conversation_id</c>).
    /// </summary>
    public ICollection<AiMessage> AiMessages { get; set; } = [];

    #endregion
}
