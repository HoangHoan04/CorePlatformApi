using System.Security.Claims;
using CorePlatformApi.Application.Common.Interfaces;

namespace CorePlatformApi.WebApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => FindGuid(ClaimTypes.NameIdentifier, "sub");

    public Guid? TenantId => FindGuid("tenant_id", "tenantId");

    public string? Username => FindValue(ClaimTypes.Name, "name", "username", "preferred_username");

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

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
