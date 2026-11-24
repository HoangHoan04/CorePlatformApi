using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Trang quản trị dữ liệu chung: cấu trúc bảng (cột, kiểu, comment, khóa, FK) và dữ liệu theo trang.
/// Tạm thời chỉ dành cho Platform Admin (đọc toàn bộ dữ liệu nền tảng).
/// </summary>
[ApiController]
[Route("api/tables")]
[Authorize]
public class TablesController(ITableDataService tables, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Danh sách bảng đã khai báo trong mô hình dữ liệu.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TableSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<TableSummary>> List()
    {
        if (!currentUser.IsPlatformAdmin) return Forbidden();
        return Ok(tables.ListTables());
    }

    /// <summary>
    /// Cấu trúc một bảng.
    /// </summary>
    [HttpGet("{schema}/{table}")]
    [ProducesResponseType<TableInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
    public ActionResult<TableInfo> Describe(string schema, string table)
    {
        if (!currentUser.IsPlatformAdmin) return Forbidden();
        var info = tables.GetTable(schema, table.Replace('-', '_'));
        return info is null ? NotFound(new ApiError("NOT_FOUND", $"Không có bảng {schema}.{table}.")) : Ok(info);
    }

    /// <summary>
    /// Dữ liệu của bảng theo trang (mới nhất trước), có tìm kiếm trên các cột chữ.
    /// </summary>
    [HttpGet("{schema}/{table}/rows")]
    [ProducesResponseType<TableRowsResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TableRowsResult>> Rows(
        string schema,
        string table,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool includeDeleted = false,
        CancellationToken ct = default)
    {
        if (!currentUser.IsPlatformAdmin) return Forbidden();
        try
        {
            return Ok(await tables.GetRowsAsync(
                schema, table.Replace('-', '_'), new TableRowsQuery(pageIndex, pageSize, search, includeDeleted), null, ct));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiError("NOT_FOUND", ex.Message));
        }
    }

    private ObjectResult Forbidden() =>
        StatusCode(StatusCodes.Status403Forbidden, new ApiError("FORBIDDEN", "Chỉ Platform Admin được xem dữ liệu này."));
}
