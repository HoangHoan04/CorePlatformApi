using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.MasterData.ExchangeRates;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers.Mdm;

/// <summary>
/// Tỷ giá quy đổi. Theo mẫu CountriesController; không có kích hoạt / ngừng hoạt động mà có xóa,
/// và GET latest để các module khác tra tỷ giá hiệu lực.
/// </summary>
[ApiController]
[Route("api/mdm/exchange-rates")]
public class ExchangeRatesController(IExchangeRateService rates) : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("pagination")]
    [HasPermission(Permissions.ExchangeRateView)]
    public async Task<ActionResult<PagedResult<ExchangeRateDto>>> Pagination([FromBody] ExchangeRateQuery query, CancellationToken ct) =>
        Ok(await rates.GetPagedAsync(query, ct));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.ExchangeRateView)]
    public async Task<ActionResult<ExchangeRateDto>> Detail(Guid id, CancellationToken ct) =>
        Ok(await rates.GetAsync(id, ct));

    [HttpPost("create")]
    [HasPermission(Permissions.ExchangeRateCreate)]
    public async Task<ActionResult<ExchangeRateDto>> Create([FromBody] ExchangeRateSaveRequest request, CancellationToken ct) =>
        Ok(await rates.CreateAsync(request, ct));

    [HttpPost("update")]
    [HasPermission(Permissions.ExchangeRateUpdate)]
    public async Task<ActionResult<ExchangeRateDto>> Update([FromBody] ExchangeRateSaveRequest request, CancellationToken ct) =>
        Ok(await rates.UpdateAsync(request, ct));

    [HttpPost("delete")]
    [HasPermission(Permissions.ExchangeRateDelete)]
    public async Task<ActionResult<bool>> Delete([FromBody] IdRequest request, CancellationToken ct)
    {
        await rates.DeleteAsync(request.Id, ct);
        return Ok(true);
    }

    /// <summary>
    /// Tỷ giá hiệu lực tại thời điểm (chỉ cần đăng nhập): ?from=USD&amp;to=VND&amp;rateType=TRANSFER&amp;at=2026-11-24T10:00:00Z
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<ExchangeRateLookupDto>> Latest([FromQuery] ExchangeRateLookupQuery query, CancellationToken ct) =>
        Ok(await rates.GetLatestAsync(query, ct));

    [HttpPost("excel/template")]
    [HasPermission(Permissions.ExchangeRateImport)]
    public async Task<IActionResult> ExcelTemplate(CancellationToken ct) =>
        File(await rates.GetTemplateAsync(ct), ExcelContentType, "Mau_nhap_ty_gia.xlsx");

    [HttpPost("excel/export")]
    [HasPermission(Permissions.ExchangeRateExport)]
    public async Task<IActionResult> ExcelExport([FromBody] ExchangeRateQuery query, CancellationToken ct) =>
        File(await rates.ExportAsync(query, ct), ExcelContentType, $"Ty_gia_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

    [HttpPost("excel/import")]
    [HasPermission(Permissions.ExchangeRateImport)]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> ExcelImport(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw AppException.BadRequest("IMPORT_FILE_REQUIRED", MessageKeys.Import.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        return Ok(await rates.ImportAsync(stream, ct));
    }
}
