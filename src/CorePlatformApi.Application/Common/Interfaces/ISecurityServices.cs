namespace CorePlatformApi.Application.Common.Interfaces;

/// <summary>
/// Băm / kiểm tra mật khẩu (PBKDF2 theo chuẩn ASP.NET Core Identity).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hash, string password);
}

/// <summary>
/// Thông tin đưa vào access token.
/// </summary>
public sealed record AccessTokenSubject(
    Guid UserId,
    Guid SessionId,
    Guid? TenantId,
    string Username,
    string FullName,
    bool IsPlatformAdmin,
    bool IsTenantAdmin);

public sealed record IssuedAccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// Phát hành JWT access token và refresh token ngẫu nhiên.
/// </summary>
public interface ITokenService
{
    IssuedAccessToken CreateAccessToken(AccessTokenSubject subject, TimeSpan lifetime);

    /// <summary>
    /// Sinh refresh token ngẫu nhiên (bản rõ trả cho client) và SHA-256 hex để lưu DB.
    /// </summary>
    (string Token, string Hash) CreateRefreshToken();

    string HashRefreshToken(string token);
}

/// <summary>
/// Thông tin kỹ thuật của database (migration, pgvector, phiên bản PostgreSQL).
/// </summary>
public interface ISystemInfoService
{
    Task<SystemInfo> GetAsync(CancellationToken cancellationToken = default);
}

public sealed record SystemInfo(
    string Database,
    string ServerVersion,
    bool CanConnect,
    int AppliedMigrations,
    int PendingMigrations,
    string? LastMigration,
    bool PgvectorEnabled);
