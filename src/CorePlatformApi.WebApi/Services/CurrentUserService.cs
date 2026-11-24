using System.Security.Claims;
using CorePlatformApi.Application.Common.Interfaces;

namespace CorePlatformApi.WebApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => FindGuid(ClaimTypes.NameIdentifier, "sub");

    public Guid? TenantId => FindGuid("tenant_id", "tenantId");

    public Guid? SessionId => FindGuid("sid");

    public bool IsPlatformAdmin => string.Equals(FindValue("platform_admin"), "true", StringComparison.OrdinalIgnoreCase);

    public string? Username => FindValue("preferred_username", ClaimTypes.Name, "username", "name");

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    /// <summary>
    /// Ngôn ngữ đầu tiên trong Accept-Language ("vi-VN,vi;q=0.9" -> "vi-VN").
    /// </summary>
    public string? Language
    {
        get
        {
            var header = httpContextAccessor.HttpContext?.Request.Headers.AcceptLanguage.ToString();
            if (string.IsNullOrWhiteSpace(header))
            {
                return null;
            }

            var first = header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            return first?.Split(';')[0].Trim() is { Length: > 0 } lang && lang != "*" ? lang : null;
        }
    }

    /// <summary>
    /// Tìm claim không phân biệt hoa thường (JwtBearer 8.x dùng CaseSensitiveClaimsIdentity).
    /// </summary>
    private string? FindValue(params string[] claimTypes)
    {
        if (User is null)
        {
            return null;
        }

        foreach (var type in claimTypes)
        {
            var claim = User.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(c.Value));
            if (claim is not null)
            {
                return claim.Value;
            }
        }

        return null;
    }

    private Guid? FindGuid(params string[] claimTypes) =>
        Guid.TryParse(FindValue(claimTypes), out var id) && id != Guid.Empty ? id : null;
}
