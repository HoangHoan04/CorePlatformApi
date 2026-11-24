using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CorePlatformApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CorePlatformApi.Infrastructure.Security;

/// <summary>
/// PBKDF2 (HMAC-SHA512, 100k vòng) theo định dạng hash của ASP.NET Core Identity v3.
/// </summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Subject = new();

    public string Hash(string password) => Hasher.HashPassword(Subject, password);

    public bool Verify(string hash, string password)
    {
        try
        {
            return Hasher.VerifyHashedPassword(Subject, hash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

/// <summary>
/// Tên claim dùng chung giữa phát hành và đọc token.
/// </summary>
public static class AppClaims
{
    public const string Subject = "sub";
    public const string SessionId = "sid";
    public const string TenantId = "tenant_id";
    public const string Username = "preferred_username";
    public const string FullName = "name";
    public const string IsPlatformAdmin = "platform_admin";
    public const string IsTenantAdmin = "tenant_admin";
}

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _issuer = configuration["JwtSettings:Issuer"] ?? JwtDefaults.Issuer;
    private readonly string _audience = configuration["JwtSettings:Audience"] ?? JwtDefaults.Audience;
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"] ?? JwtDefaults.Secret));

    public IssuedAccessToken CreateAccessToken(AccessTokenSubject subject, TimeSpan lifetime)
    {
        var now = DateTime.UtcNow;
        var expires = now.Add(lifetime);
        var claims = new List<Claim>
        {
            new(AppClaims.Subject, subject.UserId.ToString()),
            new(AppClaims.SessionId, subject.SessionId.ToString()),
            new(AppClaims.Username, subject.Username),
            new(AppClaims.FullName, subject.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        if (subject.TenantId is { } tenantId)
        {
            claims.Add(new Claim(AppClaims.TenantId, tenantId.ToString()));
        }

        if (subject.IsPlatformAdmin)
        {
            claims.Add(new Claim(AppClaims.IsPlatformAdmin, "true"));
        }

        if (subject.IsTenantAdmin)
        {
            claims.Add(new Claim(AppClaims.IsTenantAdmin, "true"));
        }

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            IssuedAt = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256),
        });

        return new IssuedAccessToken(token, new DateTimeOffset(expires, TimeSpan.Zero));
    }

    public (string Token, string Hash) CreateRefreshToken()
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(48));
        return (token, HashRefreshToken(token));
    }

    public string HashRefreshToken(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
