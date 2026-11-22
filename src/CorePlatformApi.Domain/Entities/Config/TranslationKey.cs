// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Iam;

namespace CorePlatformApi.Domain.Entities.Config;

/// <summary>
/// Khóa dịch dùng chung (nhãn UI, thông báo lỗi API) theo namespace
/// </summary>
/// <remarks>Bảng <c>config.translation_keys</c>.</remarks>
public class TranslationKey : AuditableEntity
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Nhóm: common, errors, validation, hrm, wms, crm
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Khóa (button.save, AUTH_INVALID_TOKEN)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Phân hệ
    /// </summary>
    /// <remarks>FK → <c>iam.modules(id)</c></remarks>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Ngữ cảnh cho người dịch
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Nền tảng sử dụng (lọc khi export file JSON)
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{WEB,MOBILE,API}'</c></remarks>
    public List<string> Platforms { get; set; } = ["WEB", "MOBILE", "API"];

    #region Navigations

    /// <summary>
    /// Điều hướng tới Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI (qua <c>module_id</c>).
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Danh sách Translation con (qua <c>config.translations.key_id</c>).
    /// </summary>
    public ICollection<Translation> Translations { get; set; } = [];

    #endregion
}
