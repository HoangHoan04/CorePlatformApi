using System.Reflection;

namespace CorePlatformApi.Infrastructure.Persistence.Sql;

/// <summary>
/// Đọc các script SQL nhúng (EmbeddedResource) cho những phần EF Core không biểu diễn được:
/// hàm/trigger PL/pgSQL, index biểu thức, seed dữ liệu, pgvector.
/// </summary>
public static class SqlScripts
{
    public const string CoreFunctions = "000_core_functions.sql";
    public const string ExpressionIndexes = "020_expression_indexes.sql";
    public const string UpdatedAtTriggers = "030_updated_at_triggers.sql";
    public const string DocumentNumberFunction = "040_document_number_function.sql";
    public const string SeedData = "050_seed_data.sql";
    public const string Pgvector = "060_pgvector.sql";

    public static string Load(string fileName)
    {
        var assembly = typeof(SqlScripts).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith("." + fileName, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Không tìm thấy script SQL nhúng '{fileName}'.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
