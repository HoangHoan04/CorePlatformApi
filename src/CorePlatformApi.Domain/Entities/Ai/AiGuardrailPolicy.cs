// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Chính sách guardrail: che PII, chặn chủ đề, phát hiện prompt injection, kiểm tra output
/// </summary>
/// <remarks>Bảng <c>guardrail_policies</c> (phân hệ ai).</remarks>
public class AiGuardrailPolicy : BaseEntity
{
    /// <summary>
    /// Mã chính sách
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tên
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại guardrail
    /// </summary>
    /// <remarks>Ràng buộc: <c>guard_type IN ('PII_MASK','BLOCKED_KEYWORDS','BLOCKED_TOPICS','PROMPT_INJECTION','OUTPUT_SCHEMA','MAX_INPUT_LENGTH','TOXICITY','CUSTOM')</c></remarks>
    public string GuardType { get; set; } = string.Empty;

    /// <summary>
    /// Áp dụng cho đầu vào/đầu ra
    /// </summary>
    /// <remarks>Ràng buộc: <c>apply_on IN ('INPUT','OUTPUT','BOTH')</c>; Mặc định DB: <c>'INPUT'</c></remarks>
    public string ApplyOn { get; set; } = "INPUT";

    /// <summary>
    /// Cấu hình (regex CCCD/SĐT/STK, danh sách từ khóa...)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{}'</c></remarks>
    public string Config { get; set; } = "{}";

    /// <summary>
    /// Hành động khi vi phạm
    /// </summary>
    /// <remarks>Ràng buộc: <c>action IN ('BLOCK','MASK','WARN','LOG')</c>; Mặc định DB: <c>'BLOCK'</c></remarks>
    public string Action { get; set; } = "BLOCK";

    /// <summary>
    /// Áp dụng cho mọi request của tenant
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsGlobal { get; set; } = false;

    /// <summary>
    /// Đang bật
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool IsActive { get; set; } = true;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Danh sách AiPromptGuardrail con (qua <c>ai.prompt_guardrails.guardrail_id</c>).
    /// </summary>
    public ICollection<AiPromptGuardrail> AiPromptGuardrails { get; set; } = [];

    #endregion
}
