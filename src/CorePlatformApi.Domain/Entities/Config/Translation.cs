// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Bản dịch theo ngôn ngữ; tenant có thể ghi đè thuật ngữ riêng (VD: "Nhân viên" → "Cộng sự")
/// </summary>
/// <remarks>Bảng <c>config.translations</c>.</remarks>
public class Translation : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Khóa dịch
    /// </summary>
    /// <remarks>FK → <c>config.translation_keys(id)</c></remarks>
    public Guid KeyId { get; set; }

    /// <summary>
    /// Ngôn ngữ
    /// </summary>
    /// <remarks>FK → <c>config.languages(code)</c></remarks>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// Tenant ghi đè (NULL = bản dịch chuẩn)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Nội dung dịch (hỗ trợ ICU {count, plural, ...})
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Đã duyệt
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsReviewed { get; set; } = false;

    /// <summary>
    /// Dịch máy (AI)
    /// </summary>
    /// <remarks>Mặc định DB: <c>false</c></remarks>
    public bool IsMachine { get; set; } = false;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Khóa dịch dùng chung (nhãn UI, thông báo lỗi API) theo namespace (qua <c>key_id</c>).
    /// </summary>
    public TranslationKey Key { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Ngôn ngữ hệ thống hỗ trợ (qua <c>language_code</c>).
    /// </summary>
    public Language Language { get; set; } = null!;

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}
