using System.Linq.Expressions;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CorePlatformApi.Infrastructure.Persistence;

/// <summary>
/// DbContext của nền tảng lõi: một database, mọi bảng nằm trong schema public và kế thừa <see cref="BaseEntity"/>.
/// Mô hình sinh từ docs/base_erp_core.sql (tools/sql-to-ef/gen.py): tên bảng/cột snake_case, kiểu cột,
/// comment, CHECK, UNIQUE, index, FK. Các DbSet nằm trong ApplicationDbContext.DbSets.cs (sinh tự động).
/// </summary>
public partial class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // SQL gốc tự khai báo đầy đủ index; không để EF tự sinh thêm index cho mỗi cột FK.
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // pgcrypto: gen_random_bytes(); citext: email/username; pg_trgm: index gin_trgm_ops.
        // pgvector KHÔNG khai báo ở đây vì là extension ngoài — xem Sql/060_pgvector.sql.
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!typeof(BaseEntity).IsAssignableFrom(clrType))
            {
                continue;
            }

            // Optimistic concurrency bằng cột hệ thống xmin (không tạo cột vật lý).
            modelBuilder.Entity(clrType)
                .Property(nameof(BaseEntity.Version))
                .IsRowVersion();

            // Ẩn bản ghi đã xóa mềm; dùng IgnoreQueryFilters() khi cần xem cả bản ghi đã xóa.
            var parameter = Expression.Parameter(clrType, "e");
            var body = Expression.Not(Expression.Property(parameter, nameof(BaseEntity.IsDeleted)));
            modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }
}
