using System.Data.Common;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace CorePlatformApi.Infrastructure.Persistence;

public sealed partial class TableDataService(ApplicationDbContext db) : ITableDataService
{
    /// <summary>
    /// Model đầy đủ (gồm comment, check constraint, thứ tự cột) — model runtime đã lược bỏ các thông tin này.
    /// </summary>
    private IModel Model => db.GetService<IDesignTimeModel>().Model;

    private static readonly string[] OrderCandidates =
        ["created_at", "occurred_at", "changed_at", "checked_at", "started_at", "issued_at", "usage_date"];

    /// <summary>
    /// Cột bí mật / băm / dữ liệu nhị phân lớn — không trả về giao diện.
    /// </summary>
    [GeneratedRegex(@"(_hash$|_encrypted$|^security_stamp$|^token$|^endpoint_token$|^embedding$|_tsv$)", RegexOptions.IgnoreCase)]
    private static partial Regex HiddenColumnRegex();

    public IReadOnlyList<TableSummary> ListTables() =>
        Model.GetEntityTypes()
            .Select(e => new TableSummary(
                e.GetSchema() ?? "public",
                e.GetTableName()!,
                e.ClrType.Name,
                e.GetComment(),
                e.GetProperties().Count(p => p.GetColumnName() != "xmin")))
            .OrderBy(t => t.Schema).ThenBy(t => t.Table)
            .ToList();

    public TableInfo? GetTable(string schema, string table)
    {
        var entity = FindEntity(schema, table);
        if (entity is null)
        {
            return null;
        }

        var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var pk = entity.FindPrimaryKey()?.Properties.Select(p => p.GetColumnName(store)).ToHashSet() ?? [];
        var fkTargets = entity.GetForeignKeys()
            .Where(fk => fk.Properties.Count == 1)
            .ToDictionary(
                fk => fk.Properties[0].GetColumnName(store)!,
                fk => $"{fk.PrincipalEntityType.GetSchema() ?? "public"}.{fk.PrincipalEntityType.GetTableName()}");

        var all = entity.GetProperties()
            .Select(p => (Property: p, Column: p.GetColumnName(store)))
            .Where(x => x.Column is not null && x.Column != "xmin")
            .OrderBy(x => x.Property.GetColumnOrder() ?? int.MaxValue)
            .ToList();

        var hidden = all.Where(x => IsHidden(x.Column!, x.Property.GetColumnType())).Select(x => x.Column!).ToList();
        var columns = all
            .Where(x => !hidden.Contains(x.Column!))
            .Select(x => new TableColumnInfo(
                x.Column!,
                x.Property.GetColumnType(),
                x.Property.GetComment(),
                x.Property.IsColumnNullable(store),
                pk.Contains(x.Column),
                x.Property.GetDefaultValueSql(),
                fkTargets.GetValueOrDefault(x.Column!),
                x.Property.GetComputedColumnSql() is not null))
            .ToList();

        return new TableInfo(
            entity.GetSchema() ?? "public",
            entity.GetTableName()!,
            entity.ClrType.Name,
            entity.GetComment(),
            false, // không còn bảng partition
            all.Any(x => x.Column == "tenant_id"),
            all.Any(x => x.Column == "is_deleted"),
            entity.GetCheckConstraints().Select(c => $"{c.Name}: {c.Sql}").ToList(),
            columns,
            hidden);
    }

    public async Task<TableRowsResult> GetRowsAsync(
        string schema, string table, TableRowsQuery query, Guid? tenantId, CancellationToken ct = default)
    {
        var info = GetTable(schema, table) ?? throw new KeyNotFoundException($"Không có bảng {schema}.{table}.");
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var pageIndex = Math.Max(1, query.PageIndex);

        var source = $"{Quote(info.Schema)}.{Quote(info.Table)}";
        var where = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (info.HasSoftDelete && !query.IncludeDeleted)
        {
            where.Add("NOT t.is_deleted");
        }

        if (info.HasTenant && tenantId is not null)
        {
            where.Add("t.tenant_id = @tenant");
            parameters.Add(new NpgsqlParameter("tenant", tenantId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchable = info.Columns
                .Where(c => IsTextLike(c.StoreType) || c.StoreType == "uuid")
                .Select(c => $"t.{Quote(c.Name)}::text ILIKE @search")
                .ToList();
            if (searchable.Count > 0)
            {
                where.Add("(" + string.Join(" OR ", searchable) + ")");
                parameters.Add(new NpgsqlParameter("search", "%" + EscapeLike(query.Search.Trim()) + "%"));
            }
        }

        var whereSql = where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : string.Empty;
        var orderColumn = OrderCandidates.FirstOrDefault(c => info.Columns.Any(x => x.Name == c));
        var orderSql = orderColumn is not null
            ? $"t.{Quote(orderColumn)} DESC"
            : string.Join(", ", info.Columns.Where(c => c.IsPrimaryKey).Select(c => $"t.{Quote(c.Name)}"));
        if (string.IsNullOrEmpty(orderSql))
        {
            orderSql = "1";
        }

        var columnList = string.Join(", ", info.Columns.Select(c => $"t.{Quote(c.Name)}"));
        var countSql = $"SELECT count(*) FROM {source} t{whereSql}";
        var rowsSql =
            $"SELECT coalesce(json_agg(r), '[]'::json)::text FROM (SELECT {columnList} FROM {source} t{whereSql} " +
            $"ORDER BY {orderSql} LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}) r";

        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            await connection.OpenAsync(ct);
        }

        try
        {
            var total = Convert.ToInt64(await ScalarAsync(connection, countSql, parameters, ct));
            var json = (string?)await ScalarAsync(connection, rowsSql, parameters, ct) ?? "[]";
            return new TableRowsResult(JsonNode.Parse(json)!.AsArray(), total, pageIndex, pageSize);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private IEntityType? FindEntity(string schema, string table) =>
        Model.GetEntityTypes().FirstOrDefault(e =>
            string.Equals(e.GetSchema() ?? "public", schema, StringComparison.OrdinalIgnoreCase)
            && string.Equals(e.GetTableName(), table, StringComparison.OrdinalIgnoreCase));

    private static async Task<object?> ScalarAsync(DbConnection connection, string sql, List<NpgsqlParameter> parameters, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var p in parameters)
        {
            command.Parameters.Add(p.Clone());
        }

        return await command.ExecuteScalarAsync(ct);
    }

    private static bool IsHidden(string column, string storeType) =>
        HiddenColumnRegex().IsMatch(column) || storeType.StartsWith("vector", StringComparison.Ordinal) || storeType == "tsvector";

    private static bool IsTextLike(string storeType) =>
        storeType.StartsWith("varchar", StringComparison.Ordinal) || storeType.StartsWith("char", StringComparison.Ordinal)
        || storeType is "text" or "citext";

    private static string Quote(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
