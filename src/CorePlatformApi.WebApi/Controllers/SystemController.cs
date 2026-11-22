using CorePlatformApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Thông tin trạng thái hệ thống: kết nối DB, migration đã áp dụng, thống kê schema.
/// </summary>
[ApiController]
[Route("api/system")]
public class SystemController(ApplicationDbContext db) : ControllerBase
{
    public sealed record HealthResponse(
        string Status,
        string Database,
        bool CanConnect,
        IReadOnlyList<string> AppliedMigrations,
        IReadOnlyList<string> PendingMigrations,
        bool PgvectorEnabled,
        DateTimeOffset ServerTime);

    public sealed record SchemaTableCount(string Schema, int Tables);

    public sealed record EntityMapping(string Entity, string Schema, string Table, int Columns);

    /// <summary>
    /// Kiểm tra kết nối database và trạng thái migration.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthResponse>> Health(CancellationToken ct)
    {
        var canConnect = await db.Database.CanConnectAsync(ct);
        var applied = canConnect ? (await db.Database.GetAppliedMigrationsAsync(ct)).ToList() : [];
        var pending = canConnect ? (await db.Database.GetPendingMigrationsAsync(ct)).ToList() : [];
        var pgvector = canConnect && await db.IsPgvectorEnabledAsync(ct);

        return Ok(new HealthResponse(
            canConnect && pending.Count == 0 ? "Healthy" : "Degraded",
            db.Database.GetDbConnection().Database,
            canConnect,
            applied,
            pending,
            pgvector,
            DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Số bảng thực tế trong từng schema của database.
    /// </summary>
    [HttpGet("schemas")]
    [ProducesResponseType<IReadOnlyList<SchemaTableCount>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SchemaTableCount>>> Schemas(CancellationToken ct)
    {
        var rows = await db.Database.SqlQueryRaw<SchemaTableCount>(
            """
            SELECT n.nspname AS "Schema", count(*)::int AS "Tables"
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE c.relkind IN ('r', 'p') AND NOT c.relispartition
              AND n.nspname IN ('org','iam','gateway','mdm','storage','notify','audit','monitor','workflow','integration','ai','config','job')
            GROUP BY n.nspname
            ORDER BY n.nspname
            """).ToListAsync(ct);
        return Ok(rows);
    }

    /// <summary>
    /// Danh sách entity EF Core và bảng tương ứng.
    /// </summary>
    [HttpGet("entities")]
    [ProducesResponseType<IReadOnlyList<EntityMapping>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EntityMapping>> Entities()
    {
        var items = db.Model.GetEntityTypes()
            .Select(e => new EntityMapping(
                e.ClrType.FullName!.Replace("CorePlatformApi.Domain.Entities.", string.Empty),
                e.GetSchema() ?? "public",
                e.GetTableName() ?? string.Empty,
                e.GetProperties().Count(p => p.GetColumnName() != "xmin")))
            .OrderBy(x => x.Schema).ThenBy(x => x.Table)
            .ToList();
        return Ok(items);
    }
}
