using System.Text;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Infrastructure.Persistence;
using CorePlatformApi.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CorePlatformApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var npgsqlBuilder = NpgsqlConnectionFactory.Build(configuration.GetConnectionString("DefaultConnection"));

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseNpgsql(npgsqlBuilder.ConnectionString, builder =>
            {
                builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                builder.MigrationsHistoryTable("__ef_migrations_history", "public");
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                builder.UseVector();
                builder.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });
            options.AddInterceptors(provider.GetRequiredService<AuditableEntityInterceptor>());
            options.ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // JWT (HMAC) — phát hành token sẽ do module IAM đảm nhiệm; ở đây chỉ cấu hình xác thực.
        var jwtSecret = configuration["JwtSettings:Secret"] ?? JwtDefaults.Secret;
        var issuer = configuration["JwtSettings:Issuer"] ?? JwtDefaults.Issuer;
        var audience = configuration["JwtSettings:Audience"] ?? JwtDefaults.Audience;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };
        });

        return services;
    }
}

/// <summary>
/// Giá trị mặc định cho JWT khi appsettings không cấu hình.
/// </summary>
public static class JwtDefaults
{
    public const string Secret = "SuperSecretKeyForCorePlatform2026!PleaseChangeMeInProduction";
    public const string Issuer = "CorePlatformApi";
    public const string Audience = "CorePlatform";
}
