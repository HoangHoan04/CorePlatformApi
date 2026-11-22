// Nguồn schema: docs/base_erp_core.sql
using CorePlatformApi.Domain.Common;
using CorePlatformApi.Domain.Entities.Org;

namespace CorePlatformApi.Domain.Entities.Gateway;

/// <summary>
/// Chính sách CORS (toàn hệ thống hoặc theo tenant khi tenant có domain riêng)
/// </summary>
/// <remarks>Bảng <c>gateway.cors_policies</c>.</remarks>
public class CorsPolicy : AuditableEntity, IMayHaveTenant
{
    /// <summary>
    /// Khóa chính
    /// </summary>
    /// <remarks>Mặc định DB: <c>gen_random_uuid()</c></remarks>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Tenant (NULL = mặc định)
    /// </summary>
    /// <remarks>FK → <c>org.tenants(id)</c></remarks>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Mã chính sách
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Origin cho phép (https://admin.baseerp.vn); hỗ trợ wildcard subdomain
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Method cho phép
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{GET,POST,PUT,PATCH,DELETE,OPTIONS}'</c></remarks>
    public List<string> AllowedMethods { get; set; } = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    /// <summary>
    /// Header cho phép
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{Authorization,Content-Type,X-Tenant-Id,X-Request-Id,Accept-Language}'</c></remarks>
    public List<string> AllowedHeaders { get; set; } = ["Authorization", "Content-Type", "X-Tenant-Id", "X-Request-Id", "Accept-Language"];

    /// <summary>
    /// Header trình duyệt được đọc
    /// </summary>
    /// <remarks>Mặc định DB: <c>'{X-Trace-Id,X-RateLimit-Remaining,Content-Disposition}'</c></remarks>
    public List<string> ExposedHeaders { get; set; } = ["X-Trace-Id", "X-RateLimit-Remaining", "Content-Disposition"];

    /// <summary>
    /// Cho phép cookie/credentials
    /// </summary>
    /// <remarks>Mặc định DB: <c>true</c></remarks>
    public bool AllowCredentials { get; set; } = true;

    /// <summary>
    /// Cache preflight
    /// </summary>
    /// <remarks>Mặc định DB: <c>600</c></remarks>
    public int MaxAgeSeconds { get; set; } = 600;

    #region Navigations

    /// <summary>
    /// Điều hướng tới Tenant = Tập đoàn/khách hàng thuê hệ thống (qua <c>tenant_id</c>).
    /// </summary>
    public Tenant? Tenant { get; set; }

    #endregion
}
