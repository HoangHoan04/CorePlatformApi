using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.MasterData.Currencies;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers.Mdm;

/// <summary>
/// Danh mục tiền tệ (ISO 4217). Bộ đường dẫn theo mẫu CountriesController.
/// </summary>
[ApiController]
[Route("api/mdm/currencies")]
public class CurrenciesController(ICurrencyService currencies) : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("pagination")]
    [HasPermission(Permissions.CurrencyView)]
    public async Task<ActionResult<PagedResult<CurrencyDto>>> Pagination([FromBody] CurrencyQuery query, CancellationToken ct) =>
        Ok(await currencies.GetPagedAsync(query, ct));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CurrencyView)]
    public async Task<ActionResult<CurrencyDto>> Detail(Guid id, CancellationToken ct) =>
        Ok(await currencies.GetAsync(id, ct));

    [HttpPost("create")]
    [HasPermission(Permissions.CurrencyCreate)]
    public async Task<ActionResult<CurrencyDto>> Create([FromBody] CurrencySaveRequest request, CancellationToken ct) =>
        Ok(await currencies.CreateAsync(request, ct));

    [HttpPost("update")]
    [HasPermission(Permissions.CurrencyUpdate)]
    public async Task<ActionResult<CurrencyDto>> Update([FromBody] CurrencySaveRequest request, CancellationToken ct) =>
        Ok(await currencies.UpdateAsync(request, ct));

    [HttpPost("activate")]
    [HasPermission(Permissions.CurrencyUpdate)]
    public async Task<ActionResult<bool>> Activate([FromBody] IdRequest request, CancellationToken ct)
    {
        await currencies.SetActiveAsync(request.Id, true, ct);
        return Ok(true);
    }

    [HttpPost("deactivate")]
    [HasPermission(Permissions.CurrencyUpdate)]
    public async Task<ActionResult<bool>> Deactivate([FromBody] IdRequest request, CancellationToken ct)
    {
        await currencies.SetActiveAsync(request.Id, false, ct);
        return Ok(true);
    }

    /// <summary>
    /// Tiền tệ đang hoạt động cho ô chọn (chỉ cần đăng nhập).
    /// </summary>
    [HttpGet("select-box")]
    public async Task<ActionResult<IReadOnlyList<SelectBoxItem>>> SelectBox([FromQuery] string? search, CancellationToken ct) =>
        Ok(await currencies.GetSelectBoxAsync(search, ct));

    [HttpPost("excel/template")]
    [HasPermission(Permissions.CurrencyImport)]
    public async Task<IActionResult> ExcelTemplate(CancellationToken ct) =>
        File(await currencies.GetTemplateAsync(ct), ExcelContentType, "Mau_nhap_tien_te.xlsx");

    [HttpPost("excel/export")]
    [HasPermission(Permissions.CurrencyExport)]
    public async Task<IActionResult> ExcelExport([FromBody] CurrencyQuery query, CancellationToken ct) =>
        File(await currencies.ExportAsync(query, ct), ExcelContentType, $"Tien_te_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

    [HttpPost("excel/import")]
    [HasPermission(Permissions.CurrencyImport)]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> ExcelImport(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw AppException.BadRequest("IMPORT_FILE_REQUIRED", MessageKeys.Import.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        return Ok(await currencies.ImportAsync(stream, ct));
    }
}
