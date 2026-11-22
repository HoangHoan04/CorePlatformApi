// Nguồn schema: docs/base_erp_core.sql
namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Gắn guardrail vào phiên bản prompt
/// </summary>
/// <remarks>Bảng <c>ai.prompt_guardrails</c>.</remarks>
public class AiPromptGuardrail
{
    /// <summary>
    /// Phiên bản prompt
    /// </summary>
    /// <remarks>FK → <c>ai.prompt_versions(id)</c></remarks>
    public Guid PromptVersionId { get; set; }

    /// <summary>
    /// Guardrail
    /// </summary>
    /// <remarks>FK → <c>ai.guardrail_policies(id)</c></remarks>
    public Guid GuardrailId { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Phiên bản prompt (quản lý thay đổi, rollback, A/B test) (qua <c>prompt_version_id</c>).
    /// </summary>
    public AiPromptVersion PromptVersion { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Chính sách guardrail: che PII, chặn chủ đề, phát hiện prompt injection, kiểm tra output (qua <c>guardrail_id</c>).
    /// </summary>
    public AiGuardrailPolicy Guardrail { get; set; } = null!;

    #endregion
}
