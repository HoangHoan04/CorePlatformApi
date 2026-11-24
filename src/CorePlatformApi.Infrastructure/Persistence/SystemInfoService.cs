using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Infrastructure.Persistence;

public sealed class SystemInfoService(ApplicationDbContext db) : ISystemInfoService
{
    public async Task<SystemInfo> GetAsync(CancellationToken cancellationToken = default)
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return new SystemInfo(db.Database.GetDbConnection().Database, string.Empty, false, 0, 0, null, false);
        }

        var applied = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        var version = await db.Database
            .SqlQueryRaw<string>("SELECT current_setting('server_version') AS \"Value\"")
            .SingleAsync(cancellationToken);

        return new SystemInfo(
            db.Database.GetDbConnection().Database,
            "PostgreSQL " + version,
            true,
            applied.Count,
            pending.Count,
            applied.LastOrDefault(),
            await db.IsPgvectorEnabledAsync(cancellationToken));
    }
}
