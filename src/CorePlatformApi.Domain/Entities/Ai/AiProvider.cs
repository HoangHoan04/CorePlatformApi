// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Nhà cung cấp AI (OpenAI, Gemini, Anthropic, Azure OpenAI, model nội bộ qua Ollama/vLLM)
/// </summary>
/// <remarks>Bảng <c>ai.providers</c>.</remarks>
public class AiProvider : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Mã (OPENAI, GEMINI, ANTHROPIC, AZURE_OPENAI, OLLAMA, VLLM)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Chuẩn API để chọn adapter
    /// </summary>
    /// <remarks>Ràng buộc: <c>api_style IN ('OPENAI','ANTHROPIC','GEMINI','CUSTOM')</c></remarks>
    public string ApiStyle { get; set; } = string.Empty;

    /// <summary>
    /// Base URL mặc định
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Model nội bộ (dữ liệu không rời hạ tầng)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsSelfHosted { get; set; } = false;

    /// <summary>
    /// Còn dùng
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;
}
