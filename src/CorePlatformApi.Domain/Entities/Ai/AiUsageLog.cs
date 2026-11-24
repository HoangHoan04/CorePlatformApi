// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)
using CorePlatformApi.Domain.Common;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// Log từng lần gọi AI: token, chi phí, độ trễ (không lưu nội dung prompt theo mặc định vì dữ liệu nhạy cảm)
/// </summary>
/// <remarks>Bảng <c>usage_logs</c> (phân hệ ai).</remarks>
public class AiUsageLog : BaseEntity
{
    /// <summary>
    /// Phân hệ gọi
    /// </summary>
    public string? ModuleCode { get; set; }

    /// <summary>
    /// Client nội bộ
    /// </summary>
    public Guid? GatewayClientId { get; set; }

    /// <summary>
    /// User cuối
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Prompt
    /// </summary>
    public Guid? PromptTemplateId { get; set; }

    /// <summary>
    /// Phiên bản prompt
    /// </summary>
    public Guid? PromptVersionId { get; set; }

    /// <summary>
    /// Alias được yêu cầu
    /// </summary>
    public string? AliasCode { get; set; }

    /// <summary>
    /// Model thực tế phục vụ
    /// </summary>
    public Guid? ModelId { get; set; }

    /// <summary>
    /// Key provider đã dùng
    /// </summary>
    public Guid? CredentialId { get; set; }

    /// <summary>
    /// Loại request
    /// </summary>
    /// <remarks>Ràng buộc: <c>request_type IN ('CHAT','EMBEDDING','RERANK','IMAGE','AUDIO','MODERATION')</c>; Mặc định DB: <c>'CHAT'</c></remarks>
    public string RequestType { get; set; } = "CHAT";

    /// <summary>
    /// Token đầu vào
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int InputTokens { get; set; } = 0;

    /// <summary>
    /// Token đầu ra
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int OutputTokens { get; set; } = 0;

    /// <summary>
    /// Token đọc từ cache
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int CachedTokens { get; set; } = 0;

    /// <summary>
    /// Tổng token
    /// </summary>
    /// <remarks>Cột tính toán (STORED): <c>input_tokens + output_tokens</c></remarks>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// Chi phí ước tính (USD)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public decimal EstimatedCostUsd { get; set; } = 0m;

    /// <summary>
    /// Độ trễ tổng
    /// </summary>
    public int? LatencyMs { get; set; }

    /// <summary>
    /// Thời gian tới token đầu (streaming)
    /// </summary>
    public int? TimeToFirstTokenMs { get; set; }

    /// <summary>
    /// Phục vụ bởi model fallback
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsFallback { get; set; } = false;

    /// <summary>
    /// Trả về từ semantic cache
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsCacheHit { get; set; } = false;

    /// <summary>
    /// Kết quả
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('SUCCESS','ERROR','BLOCKED','QUOTA_EXCEEDED','TIMEOUT')</c></remarks>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Mã lỗi
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Guardrail đã kích hoạt
    /// </summary>
    public List<string>? GuardrailCodes { get; set; }

    /// <summary>
    /// Trace id
    /// </summary>
    public string? TraceId { get; set; }
}
