using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CorePlatformApi.WebApi.Authorization;

/// <summary>
/// Yêu cầu quyền theo mã (iam.permissions.code), VD: <c>[HasPermission(Permissions.TranslationUpdate)]</c>.
/// Nhiều attribute trên cùng action = phải có đủ tất cả quyền.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute(PermissionPolicyProvider.Prefix + permission)
{
    public string Permission { get; } = permission;
}

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

/// <summary>
/// Tạo policy "perm:{code}" theo nhu cầu thay vì khai báo từng policy trong Program.cs.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    public const string Prefix = "perm:";

    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName[Prefix.Length..]))
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

/// <summary>
/// Platform Admin có toàn quyền; người dùng khác cần quyền hiệu lực (vai trò + ngoại lệ) chứa mã yêu cầu.
/// </summary>
public sealed class PermissionAuthorizationHandler(
    ICurrentUserService currentUser,
    IUserPermissionService permissions) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true || currentUser.UserId is not { } userId)
        {
            return;
        }

        if (currentUser.IsPlatformAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        var granted = await permissions.GetPermissionsAsync(userId);
        if (granted.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
