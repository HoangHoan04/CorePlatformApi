using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CorePlatformApi.Infrastructure.Persistence;

/// <summary>
/// Factory cho dotnet-ef (design time). Tự tạo database UTF8 nếu chưa tồn tại trước khi chạy migration.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Build();
        var logger = CreateDesignTimeLogger();

        logger.LogInformation("Checking PostgreSQL database before EF migrations...");
        DatabaseBootstrap.EnsureUtf8DatabaseAsync(configuration, logger).GetAwaiter().GetResult();

        var npgsqlBuilder = NpgsqlConnectionFactory.Build(configuration.GetConnectionString("DefaultConnection"));

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            npgsqlBuilder.ConnectionString,
            builder =>
            {
                builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                builder.MigrationsHistoryTable("__ef_migrations_history", "public");
                builder.UseVector();
                builder.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static ILogger CreateDesignTimeLogger()
    {
        return LoggerFactory
            .Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information))
            .CreateLogger("DatabaseBootstrap");
    }
}
