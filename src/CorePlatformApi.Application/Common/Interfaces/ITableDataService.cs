using System.Text.Json.Nodes;

namespace CorePlatformApi.Application.Common.Interfaces;

public sealed record TableColumnInfo(
    string Name,
    string StoreType,
    string? Comment,
    bool IsNullable,
    bool IsPrimaryKey,
    string? DefaultSql,
    string? References,
    bool IsComputed);

public sealed record TableInfo(
    string Schema,
    string Table,
    string Entity,
    string? Comment,
    bool IsPartitioned,
    bool HasTenant,
    bool HasSoftDelete,
    IReadOnlyList<string> CheckConstraints,
    IReadOnlyList<TableColumnInfo> Columns,
    IReadOnlyList<string> HiddenColumns);

public sealed record TableSummary(string Schema, string Table, string Entity, string? Comment, int Columns);

public sealed record TableRowsQuery(int PageIndex = 1, int PageSize = 20, string? Search = null, bool IncludeDeleted = false);

public sealed record TableRowsResult(JsonArray Items, long TotalCount, int PageIndex, int PageSize);

/// <summary>
/// Đọc cấu trúc và dữ liệu của mọi bảng đã khai báo trong EF model (dùng cho trang quản trị dữ liệu chung).
/// Chỉ chấp nhận schema/bảng có trong model; tên cột lấy từ model nên không thể chèn SQL.
/// </summary>
public interface ITableDataService
{
    IReadOnlyList<TableSummary> ListTables();

    TableInfo? GetTable(string schema, string table);

    Task<TableRowsResult> GetRowsAsync(string schema, string table, TableRowsQuery query, Guid? tenantId, CancellationToken ct = default);
}
