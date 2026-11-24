using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Features.Dashboard;

public sealed record DashboardStats(
    int Tenants,
    int ActiveTenants,
    int Companies,
    int Branches,
    int Departments,
    int Users,
    int ActiveUsers,
    int LockedUsers,
    int ActiveSessions,
    int Roles,
    int ActiveModules);

public sealed record LoginTrendPoint(DateOnly Date, int Success, int Failed);

public sealed record RecentLogin(
    DateTimeOffset OccurredAt,
    string? Username,
    string EventType,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent);

public sealed record ModuleItem(string Code, string Name, string? BaseRoute, bool IsCore, bool IsActive);

public sealed record DashboardOverview(
    DashboardStats Stats,
    IReadOnlyList<LoginTrendPoint> LoginTrend,
    IReadOnlyList<RecentLogin> RecentLogins,
    IReadOnlyList<ModuleItem> Modules,
    SystemInfo System,
    DateTimeOffset GeneratedAt);

public interface IDashboardService
{
    Task<DashboardOverview> GetOverviewAsync(Guid? tenantId, bool isPlatformAdmin, int days, CancellationToken ct = default);
}

/// <summary>
/// Số liệu tổng quan cho màn hình Dashboard. Platform Admin xem toàn nền tảng; user tenant chỉ thấy tenant của mình.
/// </summary>
public sealed class DashboardService(IApplicationDbContext db, ISystemInfoService systemInfo) : IDashboardService
{
    public async Task<DashboardOverview> GetOverviewAsync(Guid? tenantId, bool isPlatformAdmin, int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 90);
        var all = isPlatformAdmin;
        var now = DateTimeOffset.UtcNow;

        var tenants = db.Tenants.AsNoTracking().Where(t => all || t.Id == tenantId);
        var users = db.Users.AsNoTracking().Where(u => all || u.TenantId == tenantId);

        var stats = new DashboardStats(
            Tenants: await tenants.CountAsync(ct),
            ActiveTenants: await tenants.CountAsync(t => t.Status == "ACTIVE", ct),
            Companies: await db.Companies.AsNoTracking().CountAsync(c => all || c.TenantId == tenantId, ct),
            Branches: await db.Branches.AsNoTracking().CountAsync(b => all || b.TenantId == tenantId, ct),
            Departments: await db.Departments.AsNoTracking().CountAsync(d => all || d.TenantId == tenantId, ct),
            Users: await users.CountAsync(ct),
            ActiveUsers: await users.CountAsync(u => u.Status == "ACTIVE", ct),
            LockedUsers: await users.CountAsync(u => u.Status == "LOCKED", ct),
            ActiveSessions: await db.UserSessions.AsNoTracking()
                .CountAsync(s => (all || s.TenantId == tenantId) && s.RevokedAt == null && s.ExpiresAt > now, ct),
            Roles: await db.Roles.AsNoTracking().CountAsync(r => all || r.TenantId == tenantId || r.TenantId == null, ct),
            ActiveModules: await db.Modules.AsNoTracking().CountAsync(m => m.IsActive, ct));

        // Xu hướng đăng nhập theo ngày (giờ Việt Nam)
        var vnOffset = TimeSpan.FromHours(7);
        var fromLocalDate = DateOnly.FromDateTime(now.ToOffset(vnOffset).Date).AddDays(-(days - 1));
        var from = new DateTimeOffset(fromLocalDate.ToDateTime(TimeOnly.MinValue), vnOffset).ToUniversalTime();

        var loginEvents = await db.LoginLogs.AsNoTracking()
            .Where(l => l.OccurredAt >= from && (all || l.TenantId == tenantId)
                        && (l.EventType == "LOGIN_SUCCESS" || l.EventType == "LOGIN_FAILED" || l.EventType == "ACCOUNT_LOCKED"))
            .Select(l => new { l.OccurredAt, l.EventType })
            .ToListAsync(ct);

        var trend = Enumerable.Range(0, days)
            .Select(i => fromLocalDate.AddDays(i))
            .Select(date =>
            {
                var dayEvents = loginEvents.Where(e => DateOnly.FromDateTime(e.OccurredAt.ToOffset(vnOffset).Date) == date).ToList();
                return new LoginTrendPoint(
                    date,
                    dayEvents.Count(e => e.EventType == "LOGIN_SUCCESS"),
                    dayEvents.Count(e => e.EventType != "LOGIN_SUCCESS"));
            })
            .ToList();

        var recent = (await db.LoginLogs.AsNoTracking()
                .Where(l => all || l.TenantId == tenantId)
                .OrderByDescending(l => l.OccurredAt)
                .Take(10)
                .Select(l => new { l.OccurredAt, l.UsernameAttempted, l.UserId, l.EventType, l.FailureReason, l.IpAddress, l.UserAgent })
                .ToListAsync(ct))
            .ToList();

        var userIds = recent.Where(r => r.UserId != null).Select(r => r.UserId!.Value).Distinct().ToList();
        var usernames = await db.Users.AsNoTracking().IgnoreQueryFilters()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username, ct);

        var recentLogins = recent.Select(r => new RecentLogin(
                r.OccurredAt,
                r.UserId is { } id && usernames.TryGetValue(id, out var name) ? name : r.UsernameAttempted,
                r.EventType,
                r.FailureReason,
                r.IpAddress?.ToString(),
                r.UserAgent))
            .ToList();

        var modules = await db.Modules.AsNoTracking()
            .OrderBy(m => m.SortOrder)
            .Select(m => new ModuleItem(m.Code, m.Name, m.BaseRoute, m.IsCore, m.IsActive))
            .ToListAsync(ct);

        return new DashboardOverview(stats, trend, recentLogins, modules, await systemInfo.GetAsync(ct), now);
    }
}
