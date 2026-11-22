// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Ai;

/// <summary>
/// API key của nhà cung cấp AI — lưu tập trung, các service con không giữ key. Tenant có thể dùng key riêng (BYOK)
/// </summary>
/// <remarks>Bảng <c>ai.provider_credentials</c>.</remarks>
public class AiProviderCredential : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = key hệ thống)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Nhà cung cấp
    /// </summary>
    /// <remarks>FK → <c>ai.providers(id)</c></remarks>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Tên key
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// API key đã mã hóa
    /// </summary>
    public string ApiKeyEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// URL riêng (Azure endpoint, proxy)
    /// </summary>
    public string? BaseUrlOverride { get; set; }

    /// <summary>
    /// Org/Project id phía provider
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// Thứ tự dùng khi có nhiều key (cân bằng tải/failover)
    /// </summary>
    /// <remarks>Mặc định DB: <c>0</c></remarks>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Ngân sách tháng cho key
    /// </summary>
    public decimal? MonthlyBudgetUsd { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    /// <remarks>Ràng buộc: <c>status IN ('ACTIVE','DISABLED','EXHAUSTED','INVALID')</c>; Mặc định DB: <c>'ACTIVE'</c></remarks>
    public string Status { get; set; } = "ACTIVE";

    /// <summary>
    /// Dùng gần nhất
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// Lỗi gần nhất (401, 429...)
    /// </summary>
    public string? LastError { get; set; }

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Điều hướng tới Nhà cung cấp AI (OpenAI, Gemini, Anthropic, Azure OpenAI, model nội bộ qua Ollama/vLLM) (qua <c>provider_id</c>).
    /// </summary>
    public AiProvider Provider { get; set; } = null!;

    #endregion
}
