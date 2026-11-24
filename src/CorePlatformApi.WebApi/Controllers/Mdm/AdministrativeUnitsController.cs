using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.MasterData.AdministrativeUnits;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers.Mdm;

/// <summary>
/// Đơn vị hành chính phân cấp (Tỉnh/TP → [Quận/Huyện] → Phường/Xã). Bộ đường dẫn theo mẫu CountriesController.
/// </summary>
[ApiController]
[Route("api/mdm/administrative-units")]
public class AdministrativeUnitsController(IAdministrativeUnitService units) : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("pagination")]
    [HasPermission(Permissions.AdminUnitView)]
    public async Task<ActionResult<PagedResult<AdministrativeUnitDto>>> Pagination([FromBody] AdministrativeUnitQuery query, CancellationToken ct) =>
        Ok(await units.GetPagedAsync(query, ct));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.AdminUnitView)]
    public async Task<ActionResult<AdministrativeUnitDto>> Detail(Guid id, CancellationToken ct) =>
        Ok(await units.GetAsync(id, ct));

    [HttpPost("create")]
    [HasPermission(Permissions.AdminUnitCreate)]
    public async Task<ActionResult<AdministrativeUnitDto>> Create([FromBody] AdministrativeUnitSaveRequest request, CancellationToken ct) =>
        Ok(await units.CreateAsync(request, ct));

    [HttpPost("update")]
    [HasPermission(Permissions.AdminUnitUpdate)]
    public async Task<ActionResult<AdministrativeUnitDto>> Update([FromBody] AdministrativeUnitSaveRequest request, CancellationToken ct) =>
        Ok(await units.UpdateAsync(request, ct));

    [HttpPost("activate")]
    [HasPermission(Permissions.AdminUnitUpdate)]
    public async Task<ActionResult<bool>> Activate([FromBody] IdRequest request, CancellationToken ct)
    {
        await units.SetActiveAsync(request.Id, true, ct);
        return Ok(true);
    }

    [HttpPost("deactivate")]
    [HasPermission(Permissions.AdminUnitUpdate)]
    public async Task<ActionResult<bool>> Deactivate([FromBody] IdRequest request, CancellationToken ct)
    {
        await units.SetActiveAsync(request.Id, false, ct);
        return Ok(true);
    }

    /// <summary>
    /// Ô chọn theo tầng (chỉ cần đăng nhập): ?countryCode=VN&amp;level=1 -> tỉnh; ?parentId=... -> đơn vị con.
    /// </summary>
    [HttpGet("select-box")]
    public async Task<ActionResult<IReadOnlyList<SelectBoxItem>>> SelectBox([FromQuery] AdministrativeUnitSelectBoxQuery query, CancellationToken ct) =>
        Ok(await units.GetSelectBoxAsync(query, ct));

    [HttpPost("excel/template")]
    [HasPermission(Permissions.AdminUnitImport)]
    public async Task<IActionResult> ExcelTemplate(CancellationToken ct) =>
        File(await units.GetTemplateAsync(ct), ExcelContentType, "Mau_nhap_don_vi_hanh_chinh.xlsx");

    [HttpPost("excel/export")]
    [HasPermission(Permissions.AdminUnitExport)]
    public async Task<IActionResult> ExcelExport([FromBody] AdministrativeUnitQuery query, CancellationToken ct) =>
        File(await units.ExportAsync(query, ct), ExcelContentType, $"Don_vi_hanh_chinh_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

    [HttpPost("excel/import")]
    [HasPermission(Permissions.AdminUnitImport)]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> ExcelImport(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw AppException.BadRequest("IMPORT_FILE_REQUIRED", MessageKeys.Import.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        return Ok(await units.ImportAsync(stream, ct));
    }
}
