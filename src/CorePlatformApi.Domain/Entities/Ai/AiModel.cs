// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Model AI và bảng giá để ước tính chi phí
/// </summary>
/// <remarks>Bảng <c>models</c> (phân hệ ai).</remarks>
public class AiModel : BaseEntity
{
    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    /// <remarks>FK → <c>ai.providers(id)</c></remarks>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Mã model gửi lên API
    /// </summary>
    public string ModelCode { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Năng lực: CHAT, VISION, TOOLS, JSON_MODE, EMBEDDING, RERANK, AUDIO, IMAGE_GEN
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{CHAT}'</c></remarks>
    public List<string> Capabilities { get; set; } = ["CHAT"];

    /// <summary>
    /// Số token ngữ cảnh tối đa
    /// </summary>
    public int? ContextWindow { get; set; }

    /// <summary>
    /// Số token đầu ra tối đa
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Số chiều vector (model embedding)
    /// </summary>
    public int? EmbeddingDimension { get; set; }

    /// <summary>
    /// Giá input / 1 triệu token (USD)
    /// </summary>
    public decimal? InputPricePer1m { get; set; }

    /// <summary>
    /// Giá output / 1 triệu token (USD)
    /// </summary>
    public decimal? OutputPricePer1m { get; set; }

    /// <summary>
    /// Giá input cache / 1 triệu token
    /// </summary>
    public decimal? CachedInputPricePer1m { get; set; }

    /// <summary>
    /// Tiền tệ bảng giá
    /// </summary>
    /// <remarks>Mặc định DB: <c>'USD'</c></remarks>
    public string PriceCurrency { get; set; } = "USD";

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Ngày provider ngừng hỗ trợ
    /// </summary>
    public DateOnly? DeprecatedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Nhà cung cấp AI (OpenAI, Gemini, Anthropic, Azure OpenAI, model nội bộ qua Ollama/vLLM) (qua <c>provider_id</c>).
    /// </summary>
    public AiProvider Provider { get; set; } = null!;

    #endregion
}
