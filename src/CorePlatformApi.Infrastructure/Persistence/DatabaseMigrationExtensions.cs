using CorePlatformApi.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CorePlatformApi.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Tạo database (UTF8) nếu chưa có → áp dụng toàn bộ migration → bật pgvector nếu server đã cài.
    /// </summary>
    public static async Task MigrateWithBootstrapAsync(
        this ApplicationDbContext context,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await DatabaseBootstrap.EnsureUtf8DatabaseAsync(configuration, logger, cancellationToken);

        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count, string.Join(", ", pending));
        }

        await context.Database.MigrateAsync(cancellationToken);

        // Idempotent: chỉ thêm cột ai.kb_chunks.embedding + index HNSW khi extension pgvector có trên server.
        await context.Database.ExecuteSqlRawAsync(SqlScripts.Load(SqlScripts.Pgvector), cancellationToken);
        if (!await IsPgvectorEnabledAsync(context, cancellationToken))
        {
            logger.LogWarning(
                "pgvector chưa được cài trên PostgreSQL server: cột ai.kb_chunks.embedding chưa được tạo. " +
                "Cài pgvector rồi khởi động lại API để tự động bổ sung.");
        }
    }

    public static async Task<bool> IsPgvectorEnabledAsync(this ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        var result = await context.Database
            .SqlQueryRaw<bool>("SELECT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'vector') AS \"Value\"")
            .SingleAsync(cancellationToken);
        return result;
    }
}
