// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Phiên bản prompt (quản lý thay đổi, rollback, A/B test)
/// </summary>
/// <remarks>Bảng <c>ai.prompt_versions</c>.</remarks>
public class AiPromptVersion : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Prompt
    /// </summary>
    /// <remarks>FK → <c>ai.prompt_templates(id)</c></remarks>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Số phiên bản
    /// </summary>
    public int VersionNo { get; set; }

    /// <summary>
    /// System prompt
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// User prompt có biến {{variable}}
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Khai báo biến đầu vào, VD: [{"name":"cv_text","required":true}]
    /// </summary>
    public string? Variables { get; set; }

    /// <summary>
    /// Alias model dùng
    /// </summary>
    /// <remarks>FK → <c>ai.model_aliases(id)</c></remarks>
    public Guid? ModelAliasId { get; set; }

    /// <summary>
    /// Nhiệt độ
    /// </summary>
    public decimal? Temperature { get; set; }

    /// <summary>
    /// Top-p
    /// </summary>
    public decimal? TopP { get; set; }

    /// <summary>
    /// Giới hạn token đầu ra
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Định dạng trả về
    /// </summary>
    /// <remarks>Ràng buộc: <c>response_format IN ('TEXT','JSON','JSON_SCHEMA')</c>; Mặc định DB: <c>'TEXT'</c></remarks>
    public string ResponseFormat { get; set; } = "TEXT";

    /// <summary>
    /// JSON Schema đầu ra (JSON_SCHEMA)
    /// </summary>
    public string? JsonSchema { get; set; }

    /// <summary>
    /// Khai báo function/tool calling
    /// </summary>
    public string? Tools { get; set; }

    /// <summary>
    /// % lưu lượng (A/B test giữa các phiên bản PUBLISHED)
    /// </summary>
    /// <remarks>Ràng buộc: <c>traffic_percent BETWEEN 0 AND 100</c>; Mặc định DB: <c>100</c></remarks>
    public short TrafficPercent { get; set; } = 100;

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('DRAFT','PUBLISHED','ARCHIVED')</c>; Mặc định DB: <c>'DRAFT'</c></remarks>
    public string Status { get; set; } = "DRAFT";

    /// <summary>
    /// Ghi chú thay đổi
    /// </summary>
    public string? ChangeNote { get; set; }

    /// <summary>
    /// Phát hành lúc
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Prompt template dùng chung theo phân hệ (tóm tắt CV - HRM, phân tích KH - CRM, gợi ý đặt hàng - WMS) (qua <c>template_id</c>).
    /// </summary>
    public AiPromptTemplate Template { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Alias model logic (default-chat, cheap-summary, embedding-default) để đổi model không cần sửa code; hỗ trợ fallback (qua <c>model_alias_id</c>).
    /// </summary>
    public AiModelAlias? ModelAlias { get; set; }

    /// <summary>
    /// Danh sách AiPromptGuardrail con (qua <c>ai.prompt_guardrails.prompt_version_id</c>).
    /// </summary>
    public ICollection<AiPromptGuardrail> AiPromptGuardrails { get; set; } = [];

    #endregion
}
