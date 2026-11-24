using System.Text;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Features.Auth;
using CorePlatformApi.Infrastructure.Persistence;
using CorePlatformApi.Infrastructure.Persistence.Interceptors;
using CorePlatformApi.Infrastructure.Persistence.Seed;
using CorePlatformApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
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
        services.AddScoped<ISystemInfoService, SystemInfoService>();
        services.AddScoped<ITableDataService, TableDataService>();
        services.AddScoped<DataSeeder>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IExcelService, Excel.ExcelService>();

        // JWT (HMAC) do AuthController phát hành; mỗi request kiểm tra phiên (claim sid) chưa bị thu hồi.
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
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = AppClaims.Username,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };
            options.Events = new JwtBearerEvents
            {
                // Đăng xuất / đổi mật khẩu / khóa tài khoản / phát hiện tái sử dụng token -> phiên bị thu hồi
                // -> access token cũ hết hiệu lực ngay, không chờ hết hạn
                OnTokenValidated = async context =>
                {
                    var sid = context.Principal?.FindFirst(AppClaims.SessionId)?.Value;
                    if (!Guid.TryParse(sid, out var sessionId))
                    {
                        context.Fail(new SessionRevokedException("Token thiếu claim sid."));
                        return;
                    }

                    var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                    if (!await auth.IsSessionActiveAsync(sessionId, context.HttpContext.RequestAborted))
                    {
                        context.Fail(new SessionRevokedException("Phiên đăng nhập đã bị thu hồi."));
                    }
                },

                // 401 trả về { code, message } để client phân biệt: chưa đăng nhập / token hết hạn / phiên bị thu hồi
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    var (code, message) = context.AuthenticateFailure switch
                    {
                        SecurityTokenExpiredException => ("AUTH_TOKEN_EXPIRED", "Phiên làm việc đã hết hạn."),
                        SessionRevokedException => ("AUTH_TOKEN_REVOKED", "Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại."),
                        not null => ("AUTH_UNAUTHORIZED", "Token không hợp lệ."),
                        null => ("AUTH_UNAUTHORIZED", "Vui lòng đăng nhập."),
                    };
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    await context.Response.WriteAsJsonAsync(new { code, message }, context.HttpContext.RequestAborted);
                },

                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        new { code = "FORBIDDEN", message = "Bạn không có quyền thực hiện chức năng này." },
                        context.HttpContext.RequestAborted);
                },
            };
        });

        return services;
    }
}

/// <summary>
/// Access token hợp lệ về chữ ký nhưng phiên (claim sid) đã bị thu hồi / hết hạn / tài khoản bị khóa.
/// </summary>
public sealed class SessionRevokedException(string message) : Exception(message);

/// <summary>
/// Giá trị mặc định cho JWT khi appsettings không cấu hình.
/// </summary>
public static class JwtDefaults
{
    public const string Secret = "SuperSecretKeyForCorePlatform2026!PleaseChangeMeInProduction";
    public const string Issuer = "CorePlatformApi";
    public const string Audience = "CorePlatform";
}
