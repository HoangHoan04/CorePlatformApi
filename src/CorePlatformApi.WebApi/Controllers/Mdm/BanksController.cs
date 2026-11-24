using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.MasterData.Banks;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers.Mdm;

/// <summary>
/// Danh mục ngân hàng. Bộ đường dẫn theo mẫu CountriesController.
/// </summary>
[ApiController]
[Route("api/mdm/banks")]
public class BanksController(IBankService banks) : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("pagination")]
    [HasPermission(Permissions.BankView)]
    public async Task<ActionResult<PagedResult<BankDto>>> Pagination([FromBody] BankQuery query, CancellationToken ct) =>
        Ok(await banks.GetPagedAsync(query, ct));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.BankView)]
    public async Task<ActionResult<BankDto>> Detail(Guid id, CancellationToken ct) =>
        Ok(await banks.GetAsync(id, ct));

    [HttpPost("create")]
    [HasPermission(Permissions.BankCreate)]
    public async Task<ActionResult<BankDto>> Create([FromBody] BankSaveRequest request, CancellationToken ct) =>
        Ok(await banks.CreateAsync(request, ct));

    [HttpPost("update")]
    [HasPermission(Permissions.BankUpdate)]
    public async Task<ActionResult<BankDto>> Update([FromBody] BankSaveRequest request, CancellationToken ct) =>
        Ok(await banks.UpdateAsync(request, ct));

    [HttpPost("activate")]
    [HasPermission(Permissions.BankUpdate)]
    public async Task<ActionResult<bool>> Activate([FromBody] IdRequest request, CancellationToken ct)
    {
        await banks.SetActiveAsync(request.Id, true, ct);
        return Ok(true);
    }

    [HttpPost("deactivate")]
    [HasPermission(Permissions.BankUpdate)]
    public async Task<ActionResult<bool>> Deactivate([FromBody] IdRequest request, CancellationToken ct)
    {
        await banks.SetActiveAsync(request.Id, false, ct);
        return Ok(true);
    }

    /// <summary>
    /// Ngân hàng đang hoạt động cho ô chọn (chỉ cần đăng nhập): ?countryCode=VN&amp;supportsVietqr=true&amp;search=
    /// </summary>
    [HttpGet("select-box")]
    public async Task<ActionResult<IReadOnlyList<SelectBoxItem>>> SelectBox([FromQuery] BankSelectBoxQuery query, CancellationToken ct) =>
        Ok(await banks.GetSelectBoxAsync(query, ct));

    [HttpPost("excel/template")]
    [HasPermission(Permissions.BankImport)]
    public async Task<IActionResult> ExcelTemplate(CancellationToken ct) =>
        File(await banks.GetTemplateAsync(ct), ExcelContentType, "Mau_nhap_ngan_hang.xlsx");

    [HttpPost("excel/export")]
    [HasPermission(Permissions.BankExport)]
    public async Task<IActionResult> ExcelExport([FromBody] BankQuery query, CancellationToken ct) =>
        File(await banks.ExportAsync(query, ct), ExcelContentType, $"Ngan_hang_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

    [HttpPost("excel/import")]
    [HasPermission(Permissions.BankImport)]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> ExcelImport(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw AppException.BadRequest("IMPORT_FILE_REQUIRED", MessageKeys.Import.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        return Ok(await banks.ImportAsync(stream, ct));
    }
}
