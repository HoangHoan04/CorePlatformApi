using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Infrastructure.Persistence;
using CorePlatformApi.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Thông tin trạng thái hệ thống: kết nối DB, migration đã áp dụng, thống kê schema.
/// </summary>
[ApiController]
[Route("api/system")]
[HasPermission(Permissions.SystemView)]
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
    /// Kiểm tra kết nối database và trạng thái migration (công khai cho health check / load balancer).
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
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
              AND n.nspname = 'public'
              AND c.relname NOT LIKE '\_\_%'
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

    public sealed record PartitionInfo(string ParentTable, string Partition, string Bound, long EstimatedRows, long SizeBytes);

    public sealed record FunctionInfo(string Schema, string Name, string Arguments, string Returns, string? Comment);

    /// <summary>
    /// Partition theo tháng của các bảng log (tạo bởi core.create_monthly_partitions).
    /// </summary>
    [HttpGet("partitions")]
    [ProducesResponseType<IReadOnlyList<PartitionInfo>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PartitionInfo>>> Partitions(CancellationToken ct)
    {
        var rows = await db.Database.SqlQueryRaw<PartitionInfo>(
            """
            SELECT pn.nspname || '.' || p.relname AS "ParentTable",
                   c.relname AS "Partition",
                   pg_get_expr(c.relpartbound, c.oid) AS "Bound",
                   greatest(c.reltuples, 0)::bigint AS "EstimatedRows",
                   pg_total_relation_size(c.oid) AS "SizeBytes"
            FROM pg_inherits i
            JOIN pg_class c ON c.oid = i.inhrelid
            JOIN pg_class p ON p.oid = i.inhparent
            JOIN pg_namespace pn ON pn.oid = p.relnamespace
            WHERE p.relkind = 'p'
            ORDER BY 1, 2
            """).ToListAsync(ct);
        return Ok(rows);
    }

    /// <summary>
    /// Hàm tiện ích do hệ thống tạo trong database (bỏ qua hàm của extension như citext, pg_trgm, pgvector).
    /// </summary>
    [HttpGet("functions")]
    [ProducesResponseType<IReadOnlyList<FunctionInfo>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FunctionInfo>>> Functions(CancellationToken ct)
    {
        var rows = await db.Database.SqlQueryRaw<FunctionInfo>(
            """
            SELECT n.nspname AS "Schema", p.proname AS "Name",
                   pg_get_function_identity_arguments(p.oid) AS "Arguments",
                   pg_get_function_result(p.oid) AS "Returns",
                   obj_description(p.oid, 'pg_proc') AS "Comment"
            FROM pg_proc p
            JOIN pg_namespace n ON n.oid = p.pronamespace
            WHERE n.nspname = 'public'
              AND NOT EXISTS (SELECT 1 FROM pg_depend d WHERE d.objid = p.oid AND d.deptype = 'e')
            ORDER BY 1, 2
            """).ToListAsync(ct);
        return Ok(rows);
    }
}
