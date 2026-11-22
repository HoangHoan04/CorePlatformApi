using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Mdm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Danh mục dùng chung (đọc dữ liệu seed của nền tảng lõi).
/// </summary>
[ApiController]
[Route("api/master-data")]
public class MasterDataController(IApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Quốc gia (ISO 3166).
    /// </summary>
    [HttpGet("countries")]
    public async Task<ActionResult<List<Country>>> Countries(CancellationToken ct) =>
        await db.Countries.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Tiền tệ (ISO 4217).
    /// </summary>
    [HttpGet("currencies")]
    public async Task<ActionResult<List<Currency>>> Currencies(CancellationToken ct) =>
        await db.Currencies.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Loại thuế.
    /// </summary>
    [HttpGet("tax-types")]
    public async Task<ActionResult<List<TaxType>>> TaxTypes(CancellationToken ct) =>
        await db.TaxTypes.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    /// <summary>
    /// Thuế suất.
    /// </summary>
    [HttpGet("tax-rates")]
    public async Task<ActionResult<List<TaxRate>>> TaxRates(CancellationToken ct) =>
        await db.TaxRates.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Nhóm đơn vị tính.
    /// </summary>
    [HttpGet("uom-categories")]
    public async Task<ActionResult<List<UomCategory>>> UomCategories(CancellationToken ct) =>
        await db.UomCategories.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    /// <summary>
    /// Đơn vị tính.
    /// </summary>
    [HttpGet("uoms")]
    public async Task<ActionResult<List<Uom>>> Uoms(CancellationToken ct) =>
        await db.Uoms.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Ngôn ngữ hỗ trợ.
    /// </summary>
    [HttpGet("languages")]
    public async Task<ActionResult<List<Language>>> Languages(CancellationToken ct) =>
        await db.Languages.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Phân hệ (module) của hệ sinh thái.
    /// </summary>
    [HttpGet("modules")]
    public async Task<ActionResult<List<Module>>> Modules(CancellationToken ct) =>
        await db.Modules.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Hành động phân quyền (VIEW, CREATE, ...).
    /// </summary>
    [HttpGet("permission-actions")]
    public async Task<ActionResult<List<PermissionAction>>> PermissionActions(CancellationToken ct) =>
        await db.PermissionActions.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);

    /// <summary>
    /// Nhà cung cấp AI.
    /// </summary>
    [HttpGet("ai-providers")]
    public async Task<ActionResult<List<AiProvider>>> AiProviders(CancellationToken ct) =>
        await db.AiProviders.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);

    /// <summary>
    /// Mã lỗi chuẩn của API Gateway.
    /// </summary>
    [HttpGet("error-codes")]
    public async Task<ActionResult<List<ErrorCode>>> ErrorCodes(CancellationToken ct) =>
        await db.ErrorCodes.AsNoTracking().OrderBy(x => x.HttpStatus).ThenBy(x => x.Code).ToListAsync(ct);

    /// <summary>
    /// Chính sách rate limit của API Gateway.
    /// </summary>
    [HttpGet("rate-limit-policies")]
    public async Task<ActionResult<List<RateLimitPolicy>>> RateLimitPolicies(CancellationToken ct) =>
        await db.RateLimitPolicies.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);
}
