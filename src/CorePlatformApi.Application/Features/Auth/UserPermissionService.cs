using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Features.Auth;

/// <summary>
/// Quyền hiệu lực của người dùng = quyền từ các vai trò đang hiệu lực + quyền GRANT − quyền DENY (ngoại lệ).
/// Kết quả được nhớ trong phạm vi một request (service scoped).
/// </summary>
public interface IUserPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<RoleRef>> GetActiveRolesAsync(Guid userId, CancellationToken ct = default);
}

public sealed record RoleRef(Guid Id, string Code);

public sealed class UserPermissionService(IApplicationDbContext db) : IUserPermissionService
{
    private readonly Dictionary<Guid, IReadOnlySet<string>> _permissions = [];
    private readonly Dictionary<Guid, IReadOnlyList<RoleRef>> _roles = [];

    public async Task<IReadOnlyList<RoleRef>> GetActiveRolesAsync(Guid userId, CancellationToken ct = default)
    {
        if (_roles.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        var now = DateTimeOffset.UtcNow;
        var roles = await db.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == userId && ur.ValidFrom <= now && (ur.ValidTo == null || ur.ValidTo > now))
            .Join(db.Roles.Where(r => r.Status == "ACTIVE"), ur => ur.RoleId, r => r.Id, (ur, r) => new RoleRef(r.Id, r.Code))
            .Distinct()
            .ToListAsync(ct);

        _roles[userId] = roles;
        return roles;
    }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        if (_permissions.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        var now = DateTimeOffset.UtcNow;
        var roleIds = (await GetActiveRolesAsync(userId, ct)).Select(r => r.Id).ToList();

        var granted = roleIds.Count == 0
            ? []
            : await db.RolePermissions.AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Join(db.Permissions, rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
                .Distinct()
                .ToListAsync(ct);

        var overrides = await db.UserPermissionOverrides.AsNoTracking()
            .Where(o => o.UserId == userId && (o.ValidTo == null || o.ValidTo > now))
            .Join(db.Permissions, o => o.PermissionId, p => p.Id, (o, p) => new { o.Effect, p.Code })
            .ToListAsync(ct);

        var permissions = new HashSet<string>(granted, StringComparer.OrdinalIgnoreCase);
        permissions.UnionWith(overrides.Where(o => o.Effect == "GRANT").Select(o => o.Code));
        permissions.ExceptWith(overrides.Where(o => o.Effect == "DENY").Select(o => o.Code));

        _permissions[userId] = permissions;
        return permissions;
    }
}
