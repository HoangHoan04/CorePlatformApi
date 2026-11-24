using System.Reflection;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CorePlatformApi.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Cho mọi <see cref="BaseEntity"/>: tự gán cột audit (created_*, updated_*), chuyển <c>Remove()</c> thành xóa mềm
/// với bảng gắn <see cref="SoftDeleteAttribute"/>, và chuẩn hóa mọi DateTimeOffset về UTC
/// (Npgsql chỉ ghi timestamptz với offset 0).
/// </summary>
public sealed class AuditableEntityInterceptor(ICurrentUserService? currentUser = null) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var userId = currentUser?.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Deleted && entity.GetType().GetCustomAttribute<SoftDeleteAttribute>() is not null)
            {
                entry.State = EntityState.Modified;
                entity.IsDeleted = true;
                entity.DeletedAt = now;
                entity.DeletedBy = userId;
            }

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.CreatedBy ??= userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                entry.Property(e => e.CreatedAt).IsModified = false;
                entry.Property(e => e.CreatedBy).IsModified = false;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.CurrentValue is DateTimeOffset value && value.Offset != TimeSpan.Zero)
                    {
                        property.CurrentValue = value.ToUniversalTime();
                    }
                }
            }
        }
    }
}
