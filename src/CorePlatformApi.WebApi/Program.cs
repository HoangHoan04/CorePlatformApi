using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CorePlatformApi.Application;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Infrastructure;
using CorePlatformApi.Infrastructure.Persistence;
using CorePlatformApi.Infrastructure.Persistence.Seed;
using CorePlatformApi.WebApi.Authorization;
using CorePlatformApi.WebApi.Controllers;
using CorePlatformApi.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi;

// Hiển thị đúng tiếng Việt trong log console (Windows mặc định code page 437/1252)
Console.OutputEncoding = System.Text.Encoding.UTF8;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Ngoài môi trường Development không được chạy với khóa ký JWT mặc định / quá ngắn
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? JwtDefaults.Secret;
if (!builder.Environment.IsDevelopment() && (jwtSecret == JwtDefaults.Secret || Encoding.UTF8.GetByteCount(jwtSecret) < 32))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret phải được cấu hình (>= 32 byte, khác giá trị mặc định) khi không chạy ở môi trường Development.");
}

// Mặc định mọi endpoint đều yêu cầu đăng nhập; endpoint công khai phải gắn [AllowAnonymous].
// Phân quyền chi tiết: [HasPermission("MODULE.RESOURCE.ACTION")].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Chống dò mật khẩu: giới hạn số lần gọi đăng nhập / làm mới token theo IP
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AuthController.RateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("Auth:RateLimit:PermitLimit", 10),
            Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("Auth:RateLimit:WindowSeconds", 60)),
            QueueLimit = 0,
        }));
    options.OnRejected = async (context, ct) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ApiError("TOO_MANY_REQUESTS", "Bạn thao tác quá nhiều lần. Vui lòng thử lại sau ít phút."), ct);
    };
});

builder.Services
    .AddControllers(options => options.Filters.Add<CorePlatformApi.WebApi.Filters.AuthExceptionFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Core Platform API",
        Version = "v1",
        Description = "Nền tảng lõi BASE-ERP (org, iam, mdm, storage, notify, workflow, integration, ai, config, job, gateway, audit, monitor).",
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Nhập JWT access token",
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy => policy
        .WithOrigins(
            "http://localhost:4200",
            "http://localhost:4201",
            "http://localhost:4300",
            "http://localhost:5036")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        // Cho trình duyệt đọc tên file tải về (file mẫu / xuất Excel) và thời gian chờ khi bị giới hạn tần suất
        .WithExposedHeaders("Content-Disposition", "Retry-After"));
});

var app = builder.Build();

// Tạo database (nếu chưa có) + áp dụng migration khi khởi động.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.MigrateWithBootstrapAsync(app.Configuration, logger);
        await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database bootstrap or migration failed.");
        throw;
    }
}

// Swagger bật mặc định; tắt bằng cấu hình Swagger:Enabled=false
if (app.Configuration.GetValue("Swagger:Enabled", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Core Platform API v1");
        options.RoutePrefix = "swagger";
    });
}

// Lỗi nghiệp vụ xác thực -> { code, message } với HTTP status tương ứng
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (error is CorePlatformApi.Application.Common.Exceptions.AppException appError)
    {
        var localizer = context.RequestServices.GetRequiredService<CorePlatformApi.Application.Common.Localization.IAppLocalizer>();
        context.Response.StatusCode = appError.StatusCode;
        await context.Response.WriteAsJsonAsync(new ApiError(appError.Code,
            await localizer.GetAsync(appError.Message, appError.Args, context.RequestAborted)));
        return;
    }

    context.RequestServices.GetRequiredService<ILogger<Program>>().LogError(error, "Unhandled exception");
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ApiError("INTERNAL_ERROR", "Hệ thống gặp lỗi, vui lòng thử lại sau."));
}));

app.UseCors("AllowAngularDev");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
