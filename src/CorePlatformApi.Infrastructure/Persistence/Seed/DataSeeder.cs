using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Common.Security;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CorePlatformApi.Infrastructure.Persistence.Seed;

/// <summary>
/// Dữ liệu khởi tạo khi API chạy:
///  1. Danh mục quyền của module CORE (iam.resources + iam.permissions) theo <see cref="Permissions.CoreResources"/>
///     — luôn chạy, chỉ thêm phần còn thiếu.
///  2. Tài khoản Platform Admin đầu tiên khi chưa có (Seed:Enabled, Seed:Admin:*). Không ghi đè mật khẩu đã có.
/// </summary>
public sealed class DataSeeder(ApplicationDbContext db, IPasswordHasher passwordHasher, IConfiguration configuration, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
        await SeedPlatformAdminAsync(ct);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var module = await db.Modules.SingleOrDefaultAsync(m => m.Code == Permissions.CoreModule, ct);
        if (module is null)
        {
            logger.LogWarning("Chưa có module {Module} trong iam.modules — bỏ qua seed quyền.", Permissions.CoreModule);
            return;
        }

        var actions = await db.PermissionActions.AsNoTracking().ToDictionaryAsync(a => a.Code, a => a.Name, ct);
        var resources = await db.Resources.Where(r => r.ModuleId == module.Id).ToDictionaryAsync(r => r.Code, ct);
        var existingCodes = (await db.Permissions.Select(p => p.Code).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var sort = resources.Count;
        foreach (var def in Permissions.CoreResources)
        {
            if (!resources.TryGetValue(def.Code, out var resource))
            {
                resource = new Resource
                {
                    ModuleId = module.Id,
                    Code = def.Code,
                    Name = def.Name,
                    Route = def.Route,
                    SupportsDataScope = false,
                    SortOrder = ++sort,
                };
                db.Resources.Add(resource);
                resources[def.Code] = resource;
            }

            foreach (var action in def.Actions)
            {
                var code = Permissions.Code(def.Code, action);
                if (existingCodes.Contains(code))
                {
                    continue;
                }

                if (!actions.TryGetValue(action, out var actionName))
                {
                    logger.LogWarning("Hành động {Action} chưa có trong iam.permission_actions — bỏ qua quyền {Code}.", action, code);
                    continue;
                }

                db.Permissions.Add(new Permission
                {
                    ModuleId = module.Id,
                    ResourceId = resource.Id,
                    ActionCode = action,
                    Code = code,
                    Name = $"{actionName} {def.Name.ToLowerInvariant()}",
                    IsSystem = true,
                });
                existingCodes.Add(code);
                added++;
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Đã seed {Count} quyền của module {Module}.", added, Permissions.CoreModule);
        }
    }

    private async Task SeedPlatformAdminAsync(CancellationToken ct)
    {
        if (!configuration.GetValue("Seed:Enabled", false))
        {
            return;
        }

        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.IsPlatformAdmin, ct))
        {
            return;
        }

        var username = configuration["Seed:Admin:Username"] ?? "admin";
        var password = configuration["Seed:Admin:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:Admin:Password chưa cấu hình — bỏ qua tạo tài khoản Platform Admin.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        db.Users.Add(new User
        {
            TenantId = null,
            Username = username,
            Email = configuration["Seed:Admin:Email"],
            FullName = configuration["Seed:Admin:FullName"] ?? "Quản trị nền tảng",
            DisplayName = "Platform Admin",
            JobTitle = "Quản trị hệ thống",
            PasswordHash = passwordHasher.Hash(password),
            PasswordChangedAt = now,
            MustChangePassword = configuration.GetValue("Seed:Admin:MustChangePassword", false),
            UserType = "INTERNAL",
            IsPlatformAdmin = true,
            Status = "ACTIVE",
            ActivatedAt = now,
            Locale = "vi-VN",
            Timezone = "Asia/Ho_Chi_Minh",
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Đã tạo tài khoản Platform Admin '{Username}'.", username);
    }
}
