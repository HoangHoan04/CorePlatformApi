using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.Application.Common.Models;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Application.Features.MasterData.AdministrativeUnitMappings;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers.Mdm;

/// <summary>
/// Ánh xạ đơn vị hành chính cũ → mới (sáp nhập / chia tách / đổi tên). Theo mẫu CountriesController;
/// không có kích hoạt / ngừng hoạt động (bảng không có trạng thái) mà có xóa.
/// </summary>
[ApiController]
[Route("api/mdm/administrative-unit-mappings")]
public class AdministrativeUnitMappingsController(IAdministrativeUnitMappingService mappings) : ControllerBase
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpPost("pagination")]
    [HasPermission(Permissions.AdminUnitMappingView)]
    public async Task<ActionResult<PagedResult<AdministrativeUnitMappingDto>>> Pagination([FromBody] AdministrativeUnitMappingQuery query, CancellationToken ct) =>
        Ok(await mappings.GetPagedAsync(query, ct));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.AdminUnitMappingView)]
    public async Task<ActionResult<AdministrativeUnitMappingDto>> Detail(Guid id, CancellationToken ct) =>
        Ok(await mappings.GetAsync(id, ct));

    [HttpPost("create")]
    [HasPermission(Permissions.AdminUnitMappingCreate)]
    public async Task<ActionResult<AdministrativeUnitMappingDto>> Create([FromBody] AdministrativeUnitMappingSaveRequest request, CancellationToken ct) =>
        Ok(await mappings.CreateAsync(request, ct));

    [HttpPost("update")]
    [HasPermission(Permissions.AdminUnitMappingUpdate)]
    public async Task<ActionResult<AdministrativeUnitMappingDto>> Update([FromBody] AdministrativeUnitMappingSaveRequest request, CancellationToken ct) =>
        Ok(await mappings.UpdateAsync(request, ct));

    [HttpPost("delete")]
    [HasPermission(Permissions.AdminUnitMappingDelete)]
    public async Task<ActionResult<bool>> Delete([FromBody] IdRequest request, CancellationToken ct)
    {
        await mappings.DeleteAsync(request.Id, ct);
        return Ok(true);
    }

    [HttpPost("excel/template")]
    [HasPermission(Permissions.AdminUnitMappingImport)]
    public async Task<IActionResult> ExcelTemplate(CancellationToken ct) =>
        File(await mappings.GetTemplateAsync(ct), ExcelContentType, "Mau_nhap_anh_xa_sap_nhap.xlsx");

    [HttpPost("excel/export")]
    [HasPermission(Permissions.AdminUnitMappingExport)]
    public async Task<IActionResult> ExcelExport([FromBody] AdministrativeUnitMappingQuery query, CancellationToken ct) =>
        File(await mappings.ExportAsync(query, ct), ExcelContentType, $"Anh_xa_sap_nhap_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

    [HttpPost("excel/import")]
    [HasPermission(Permissions.AdminUnitMappingImport)]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> ExcelImport(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw AppException.BadRequest("IMPORT_FILE_REQUIRED", MessageKeys.Import.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        return Ok(await mappings.ImportAsync(stream, ct));
    }
}
