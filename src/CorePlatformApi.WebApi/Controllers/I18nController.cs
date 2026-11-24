using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.Localization;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Quản trị đa ngôn ngữ: cài đặt ngôn ngữ (config.languages), khóa dịch + bản dịch
/// (config.translation_keys / config.translations), đồng bộ khóa từ ứng dụng, nhập/xuất JSON.
/// Bản dịch dùng cho giao diện vẫn lấy qua GET /api/translations/{lang}.
/// Bản dịch chuẩn dùng chung cho mọi tenant: chỉ người có quyền CORE.LANGUAGE.* / CORE.TRANSLATION.* (hoặc Platform Admin).
/// </summary>
[ApiController]
[Route("api/i18n")]
[Authorize]
public class I18nController(
    ILanguageService languages,
    ITranslationAdminService translations) : ControllerBase
{
    private static readonly JsonSerializerOptions ExportJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    #region Ngôn ngữ

    /// <summary>
    /// Ngôn ngữ đang hỗ trợ (cho bộ chọn ngôn ngữ; không cần đăng nhập).
    /// </summary>
    [HttpGet("languages/active")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ActiveLanguageDto>>> ActiveLanguages(CancellationToken ct) =>
        Ok(await languages.GetActiveAsync(ct));

    /// <summary>
    /// Danh sách ngôn ngữ theo trang, kèm tiến độ dịch.
    /// </summary>
    [HttpPost("languages/pagination")]
    [HasPermission(Permissions.LanguageView)]
    public async Task<ActionResult<PagedResult<LanguageDto>>> LanguagePagination([FromBody] LanguageQuery query, CancellationToken ct) =>
        Ok(await languages.GetPagedAsync(query, ct));

    /// <summary>
    /// Chi tiết một ngôn ngữ.
    /// </summary>
    [HttpGet("languages/{code}")]
    [HasPermission(Permissions.LanguageView)]
    public async Task<ActionResult<LanguageDto>> LanguageDetail(string code, CancellationToken ct) =>
        Ok(await languages.GetAsync(code, ct));

    [HttpPost("languages/create")]
    [HasPermission(Permissions.LanguageCreate)]
    public async Task<ActionResult<LanguageDto>> CreateLanguage([FromBody] LanguageSaveRequest request, CancellationToken ct) =>
        Ok(await languages.CreateAsync(request, ct));

    [HttpPost("languages/update")]
    [HasPermission(Permissions.LanguageUpdate)]
    public async Task<ActionResult<LanguageDto>> UpdateLanguage([FromBody] LanguageSaveRequest request, CancellationToken ct) =>
        Ok(await languages.UpdateAsync(request, ct));

    [HttpPost("languages/activate")]
    [HasPermission(Permissions.LanguageUpdate)]
    public async Task<ActionResult<bool>> ActivateLanguage([FromBody] LanguageCodeRequest request, CancellationToken ct)
    {
        await languages.SetActiveAsync(request.Code, true, ct);
        return Ok(true);
    }

    [HttpPost("languages/deactivate")]
    [HasPermission(Permissions.LanguageUpdate)]
    public async Task<ActionResult<bool>> DeactivateLanguage([FromBody] LanguageCodeRequest request, CancellationToken ct)
    {
        await languages.SetActiveAsync(request.Code, false, ct);
        return Ok(true);
    }

    [HttpPost("languages/set-default")]
    [HasPermission(Permissions.LanguageUpdate)]
    public async Task<ActionResult<bool>> SetDefaultLanguage([FromBody] LanguageCodeRequest request, CancellationToken ct)
    {
        await languages.SetDefaultAsync(request.Code, ct);
        return Ok(true);
    }

    /// <summary>
    /// Xóa ngôn ngữ cùng toàn bộ bản dịch của ngôn ngữ đó.
    /// </summary>
    [HttpPost("languages/delete")]
    [HasPermission(Permissions.LanguageDelete)]
    public async Task<ActionResult<bool>> DeleteLanguage([FromBody] LanguageCodeRequest request, CancellationToken ct)
    {
        await languages.DeleteAsync(request.Code, ct);
        return Ok(true);
    }

    #endregion

    #region Bản dịch

    /// <summary>
    /// Danh sách khóa dịch theo trang, mỗi dòng kèm bản dịch của mọi ngôn ngữ.
    /// </summary>
    [HttpPost("translations/pagination")]
    [HasPermission(Permissions.TranslationView)]
    public async Task<ActionResult<PagedResult<TranslationRowDto>>> TranslationPagination([FromBody] TranslationQuery query, CancellationToken ct) =>
        Ok(await translations.GetPagedAsync(query, ct));

    /// <summary>
    /// Các namespace (nhóm khóa) và số khóa của mỗi nhóm.
    /// </summary>
    [HttpGet("translations/namespaces")]
    [HasPermission(Permissions.TranslationView)]
    public async Task<ActionResult<IReadOnlyList<NamespaceCountDto>>> Namespaces(CancellationToken ct) =>
        Ok(await translations.GetNamespacesAsync(ct));

    /// <summary>
    /// Tổng số khóa và số khóa đã dịch theo ngôn ngữ.
    /// </summary>
    [HttpGet("translations/stats")]
    [HasPermission(Permissions.TranslationView)]
    public async Task<ActionResult<TranslationStatsDto>> Stats(CancellationToken ct) =>
        Ok(await translations.GetStatsAsync(ct));

    /// <summary>
    /// Đồng bộ khóa: khóa ứng dụng gửi lên + khóa API dùng (MessageKeys: thông báo lỗi, tiêu đề cột Excel).
    /// Chỉ tạo khóa còn thiếu với bản dịch rỗng, không ghi đè bản dịch đã có.
    /// </summary>
    [HttpPost("translations/sync")]
    [HasPermission(Permissions.TranslationImport)]
    public async Task<ActionResult<TranslationSyncResult>> Sync([FromBody] TranslationSyncRequest request, CancellationToken ct)
    {
        await translations.SyncKeysAsync(new TranslationSyncRequest
        {
            Platform = "API",
            Keys = MessageKeys.All().Select(k => new TranslationSyncItem { Key = k, Description = "Dùng ở API" }).ToList(),
        }, ct);
        return Ok(await translations.SyncKeysAsync(request, ct));
    }

    /// <summary>
    /// Lưu hàng loạt bản dịch (giá trị rỗng = xóa bản dịch).
    /// </summary>
    [HttpPost("translations/save")]
    [HasPermission(Permissions.TranslationUpdate)]
    public async Task<ActionResult<TranslationSaveResult>> Save([FromBody] TranslationSaveRequest request, CancellationToken ct) =>
        Ok(await translations.SaveTranslationsAsync(request, ct));

    /// <summary>
    /// Tạo / sửa một khóa dịch (Id rỗng = tạo mới).
    /// </summary>
    [HttpPost("translations/keys/save")]
    [HasPermission(Permissions.TranslationUpdate)]
    public async Task<ActionResult<TranslationRowDto>> SaveKey([FromBody] TranslationKeySaveRequest request, CancellationToken ct) =>
        Ok(await translations.SaveKeyAsync(request, ct));

    /// <summary>
    /// Xóa khóa dịch cùng toàn bộ bản dịch của khóa.
    /// </summary>
    [HttpPost("translations/keys/delete")]
    [HasPermission(Permissions.TranslationUpdate)]
    public async Task<ActionResult<int>> DeleteKeys([FromBody] IdsRequest request, CancellationToken ct) =>
        Ok(await translations.DeleteKeysAsync(request.Ids, ct));

    /// <summary>
    /// Nhập bản dịch một ngôn ngữ từ JSON (lồng nhau hoặc phẳng).
    /// </summary>
    [HttpPost("translations/import")]
    [HasPermission(Permissions.TranslationImport)]
    public async Task<ActionResult<TranslationImportResult>> Import([FromBody] TranslationImportRequest request, CancellationToken ct) =>
        Ok(await translations.ImportAsync(request, ct));

    /// <summary>
    /// Xuất bản dịch một ngôn ngữ ra file JSON phẳng { "namespace.key": "giá trị" } (gồm cả khóa chưa dịch).
    /// </summary>
    [HttpGet("translations/export/{lang}")]
    [HasPermission(Permissions.TranslationExport)]
    public async Task<IActionResult> Export(string lang, CancellationToken ct)
    {
        var data = await translations.ExportFlatAsync(lang, ct);
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, ExportJsonOptions));
        return File(bytes, "application/json", $"translations_{lang}_{DateTime.Now:yyyyMMdd_HHmm}.json");
    }

    #endregion
}
