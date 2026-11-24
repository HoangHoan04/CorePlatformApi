using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.MasterData.Countries;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers.Mdm;

/// <summary>
/// MẪU CRUD CHUẨN — danh mục quốc gia. Quy ước đường dẫn cho mọi danh mục:
/// <list type="bullet">
/// <item>POST pagination · GET {id} · POST create · POST update · POST activate · POST deactivate</item>
/// <item>GET select-box (chỉ cần đăng nhập — dùng cho ô chọn ở màn hình khác)</item>
/// <item>POST excel/template · POST excel/export (body = bộ lọc) · POST excel/import (multipart, field "file")</item>
/// </list>
/// </summary>
[ApiController]
[Route("api/mdm/countries")]
public class CountriesController(ICountryService countries) : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("pagination")]
    [HasPermission(Permissions.CountryView)]
    public async Task<ActionResult<PagedResult<CountryDto>>> Pagination([FromBody] CountryQuery query, CancellationToken ct) =>
        Ok(await countries.GetPagedAsync(query, ct));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CountryView)]
    public async Task<ActionResult<CountryDto>> Detail(Guid id, CancellationToken ct) =>
        Ok(await countries.GetAsync(id, ct));

    [HttpPost("create")]
    [HasPermission(Permissions.CountryCreate)]
    public async Task<ActionResult<CountryDto>> Create([FromBody] CountrySaveRequest request, CancellationToken ct) =>
        Ok(await countries.CreateAsync(request, ct));

    [HttpPost("update")]
    [HasPermission(Permissions.CountryUpdate)]
    public async Task<ActionResult<CountryDto>> Update([FromBody] CountrySaveRequest request, CancellationToken ct) =>
        Ok(await countries.UpdateAsync(request, ct));

    [HttpPost("activate")]
    [HasPermission(Permissions.CountryUpdate)]
    public async Task<ActionResult<bool>> Activate([FromBody] IdRequest request, CancellationToken ct)
    {
        await countries.SetActiveAsync(request.Id, true, ct);
        return Ok(true);
    }

    [HttpPost("deactivate")]
    [HasPermission(Permissions.CountryUpdate)]
    public async Task<ActionResult<bool>> Deactivate([FromBody] IdRequest request, CancellationToken ct)
    {
        await countries.SetActiveAsync(request.Id, false, ct);
        return Ok(true);
    }

    /// <summary>
    /// Quốc gia đang hoạt động cho ô chọn (không cần quyền xem danh mục, chỉ cần đăng nhập).
    /// </summary>
    [HttpGet("select-box")]
    public async Task<ActionResult<IReadOnlyList<SelectBoxItem>>> SelectBox([FromQuery] string? search, CancellationToken ct) =>
        Ok(await countries.GetSelectBoxAsync(search, ct));

    [HttpPost("excel/template")]
    [HasPermission(Permissions.CountryImport)]
    public async Task<IActionResult> ExcelTemplate(CancellationToken ct) =>
        File(await countries.GetTemplateAsync(ct), ExcelContentType, "Mau_nhap_quoc_gia.xlsx");

    [HttpPost("excel/export")]
    [HasPermission(Permissions.CountryExport)]
    public async Task<IActionResult> ExcelExport([FromBody] CountryQuery query, CancellationToken ct) =>
        File(await countries.ExportAsync(query, ct), ExcelContentType, $"Quoc_gia_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

    [HttpPost("excel/import")]
    [HasPermission(Permissions.CountryImport)]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> ExcelImport(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw AppException.BadRequest("IMPORT_FILE_REQUIRED", MessageKeys.Import.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        return Ok(await countries.ImportAsync(stream, ct));
    }
}
