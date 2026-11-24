namespace CorePlatformApi.Application.Features.Auth;

public sealed class LoginRequest
{
    /// <summary>
    /// Tên đăng nhập hoặc email.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Mã đơn vị (org.tenants.code). Bỏ trống với Platform Admin hoặc khi username là duy nhất.
    /// </summary>
    public string? TenantCode { get; set; }
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    bool MustChangePassword,
    CurrentUserDto User);

public sealed record CurrentUserDto(
    Guid Id,
    string Username,
    string FullName,
    string? DisplayName,
    string? Email,
    string? JobTitle,
    Guid? TenantId,
    string? TenantCode,
    string? TenantName,
    string UserType,
    bool IsPlatformAdmin,
    bool IsTenantAdmin,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record RequestContext(string? IpAddress, string? UserAgent);

/// <summary>
/// Lỗi nghiệp vụ xác thực, ánh xạ sang HTTP status + mã lỗi chuẩn (gateway.error_codes).
/// </summary>
public sealed class AuthException(int statusCode, string code, string message)
    : Common.Exceptions.AppException(statusCode, code, message);
