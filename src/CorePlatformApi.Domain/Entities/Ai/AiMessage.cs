// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Tin nhắn trong hội thoại, kèm trích dẫn chunk nguồn và đánh giá của người dùng
/// </summary>
/// <remarks>Bảng <c>messages</c> (phân hệ ai).</remarks>
public class AiMessage : BaseEntity
{
    /// <summary>
    /// Hội thoại
    /// </summary>
    /// <remarks>FK → <c>ai.conversations(id)</c></remarks>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Vai trò
    /// </summary>
    /// <remarks>Ràng buộc: <c>role IN ('USER','ASSISTANT','SYSTEM','TOOL')</c></remarks>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Nguồn trích dẫn [{"chunk_id":"...","document_id":"...","score":0.82}]
    /// </summary>
    public string? Citations { get; set; }

    /// <summary>
    /// Liên kết ai.usage_logs
    /// </summary>
    public Guid? UsageLogId { get; set; }

    /// <summary>
    /// Token vào
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// Token ra
    /// </summary>
    public int? OutputTokens { get; set; }

    /// <summary>
    /// Đánh giá: 1 hữu ích, -1 không hữu ích
    /// </summary>
    /// <remarks>Ràng buộc: <c>feedback IN (-1, 1)</c></remarks>
    public short? Feedback { get; set; }

    /// <summary>
    /// Góp ý
    /// </summary>
    public string? FeedbackNote { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Hội thoại chatbot nội bộ (qua <c>conversation_id</c>).
    /// </summary>
    public AiConversation Conversation { get; set; } = null!;

    #endregion
}
