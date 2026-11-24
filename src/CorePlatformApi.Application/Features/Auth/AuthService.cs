using System.Net;
using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Domain.Entities.Audit;
using CorePlatformApi.Domain.Entities.Iam;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, RequestContext context, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, RequestContext context, CancellationToken ct = default);

    Task LogoutAsync(Guid userId, Guid sessionId, RequestContext context, CancellationToken ct = default);

    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);

    Task ChangePasswordAsync(Guid userId, Guid sessionId, ChangePasswordRequest request, CancellationToken ct = default);

    Task<bool> IsSessionActiveAsync(Guid sessionId, CancellationToken ct = default);
}

/// <summary>
/// Đăng nhập bằng mật khẩu trên mô hình iam.* : khóa tài khoản theo security policy, phiên (user_sessions),
/// refresh token xoay vòng có phát hiện tái sử dụng (refresh_tokens.family_id), nhật ký audit.login_logs.
/// </summary>
public sealed class AuthService(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUserPermissionService permissionService) : IAuthService
{
    private const string InvalidCredentialsMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";

    public async Task<AuthResponse> LoginAsync(LoginRequest request, RequestContext context, CancellationToken ct = default)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        if (username.Length == 0 || string.IsNullOrEmpty(request.Password))
        {
            throw new AuthException(400, "VALIDATION_FAILED", "Vui lòng nhập tên đăng nhập và mật khẩu.");
        }

        var user = await FindUserAsync(username, request.TenantCode?.Trim(), ct);
        var now = DateTimeOffset.UtcNow;

        if (user is null)
        {
            await WriteLoginLogAsync(null, null, username, "LOGIN_FAILED", "USER_NOT_FOUND", null, context, ct);
            throw new AuthException(401, "AUTH_UNAUTHORIZED", InvalidCredentialsMessage);
        }

        var policy = await GetPolicyAsync(user.TenantId, ct);

        // Hết thời gian khóa tạm -> tự mở khóa
        if (user.Status == "LOCKED" && user.LockedUntil is { } until && until <= now)
        {
            user.Status = "ACTIVE";
            user.FailedLoginCount = 0;
            user.LockedAt = null;
            user.LockedUntil = null;
            user.LockedReason = null;
        }

        switch (user.Status)
        {
            case "LOCKED":
                await WriteLoginLogAsync(user.TenantId, user.Id, username, "LOGIN_FAILED", "ACCOUNT_LOCKED", null, context, ct);
                throw new AuthException(423, "AUTH_ACCOUNT_LOCKED", user.LockedUntil is { } lockedUntil
                    ? $"Tài khoản đang bị khóa tạm đến {lockedUntil.ToLocalTime():HH:mm dd/MM/yyyy} do đăng nhập sai nhiều lần."
                    : "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
            case "DISABLED":
            case "PENDING":
                await WriteLoginLogAsync(user.TenantId, user.Id, username, "LOGIN_FAILED", "ACCOUNT_" + user.Status, null, context, ct);
                throw new AuthException(403, "FORBIDDEN", user.Status == "PENDING"
                    ? "Tài khoản chưa được kích hoạt."
                    : "Tài khoản đã bị vô hiệu hóa.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            user.FailedLoginCount++;
            user.LastFailedLoginAt = now;
            var locked = user.FailedLoginCount >= policy.MaxFailedAttempts;
            if (locked)
            {
                user.Status = "LOCKED";
                user.LockedAt = now;
                user.LockedUntil = now.AddMinutes(policy.LockoutMinutes);
                user.LockedReason = $"Đăng nhập sai {user.FailedLoginCount} lần liên tiếp";
            }

            await WriteLoginLogAsync(user.TenantId, user.Id, username, locked ? "ACCOUNT_LOCKED" : "LOGIN_FAILED", "INVALID_PASSWORD", null, context, ct);

            if (locked)
            {
                throw new AuthException(423, "AUTH_ACCOUNT_LOCKED",
                    $"Bạn đã nhập sai mật khẩu {user.FailedLoginCount} lần. Tài khoản bị khóa tạm {policy.LockoutMinutes} phút.");
            }

            var remaining = policy.MaxFailedAttempts - user.FailedLoginCount;
            throw new AuthException(401, "AUTH_UNAUTHORIZED",
                remaining <= 2 ? $"{InvalidCredentialsMessage} Còn {remaining} lần thử trước khi tài khoản bị khóa." : InvalidCredentialsMessage);
        }

        if (user.MfaEnabled)
        {
            await WriteLoginLogAsync(user.TenantId, user.Id, username, "LOGIN_FAILED", "MFA_NOT_SUPPORTED", null, context, ct);
            throw new AuthException(403, "FORBIDDEN", "Tài khoản đã bật xác thực 2 lớp; phương thức này chưa được hỗ trợ trên ứng dụng.");
        }

        user.FailedLoginCount = 0;
        user.LastLoginAt = now;
        user.LastLoginIp = ParseIp(context.IpAddress);

        var session = new UserSession
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            ClientId = "core-admin-web",
            AuthMethod = "PASSWORD",
            CompanyId = user.DefaultCompanyId,
            BranchId = user.DefaultBranchId,
            IpAddress = ParseIp(context.IpAddress),
            UserAgent = Truncate(context.UserAgent, 1000),
            StartedAt = now,
            LastActivityAt = now,
            ExpiresAt = now.AddDays(policy.RefreshTokenDays),
        };
        db.UserSessions.Add(session);

        var familyId = Guid.CreateVersion7();
        var (refreshToken, refreshEntity) = NewRefreshToken(session, familyId, null, context, now, policy.RefreshTokenDays);
        db.RefreshTokens.Add(refreshEntity);

        await WriteLoginLogAsync(user.TenantId, user.Id, username, "LOGIN_SUCCESS", null, session.Id, context, ct);

        var access = tokenService.CreateAccessToken(ToSubject(user, session.Id), TimeSpan.FromMinutes(policy.AccessTokenMinutes));
        var profile = await GetCurrentUserAsync(user.Id, ct);

        return new AuthResponse(access.Token, access.ExpiresAt, refreshToken, refreshEntity.ExpiresAt, user.MustChangePassword, profile);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, RequestContext context, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new AuthException(400, "VALIDATION_FAILED", "Thiếu refresh token.");
        }

        var now = DateTimeOffset.UtcNow;
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new AuthException(401, "AUTH_TOKEN_REVOKED", "Phiên đăng nhập không hợp lệ.");

        var session = await db.UserSessions.SingleAsync(s => s.Id == token.SessionId, ct);

        // Token đã dùng/thu hồi mà vẫn bị gửi lại -> nghi bị đánh cắp: thu hồi toàn bộ họ token + phiên
        if (token.UsedAt is not null || token.RevokedAt is not null)
        {
            var family = await db.RefreshTokens.Where(t => t.FamilyId == token.FamilyId && t.RevokedAt == null).ToListAsync(ct);
            foreach (var t in family)
            {
                t.RevokedAt = now;
                t.RevokedReason = "TOKEN_REUSE";
            }

            RevokeSession(session, "TOKEN_REUSE", null, now);
            await WriteLoginLogAsync(session.TenantId, token.UserId, null, "TOKEN_REUSE_DETECTED", null, session.Id, context, ct);
            throw new AuthException(401, "AUTH_TOKEN_REVOKED", "Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại.");
        }

        if (token.ExpiresAt <= now || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            throw new AuthException(401, "AUTH_TOKEN_EXPIRED", "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == token.UserId, ct);
        if (user is null || user.Status != "ACTIVE")
        {
            RevokeSession(session, "ACCOUNT_LOCKED", null, now);
            await RevokeSessionTokensAsync(session.Id, "ACCOUNT_LOCKED", now, ct);
            await db.SaveChangesAsync(ct);
            throw new AuthException(401, "AUTH_ACCOUNT_LOCKED", "Tài khoản không còn hoạt động.");
        }

        var policy = await GetPolicyAsync(user.TenantId, ct);

        // Không thao tác quá thời gian cho phép (security_policies.session_idle_minutes) -> phiên hết hạn
        if (policy.SessionIdleMinutes is > 0 && session.LastActivityAt.AddMinutes(policy.SessionIdleMinutes.Value) <= now)
        {
            RevokeSession(session, "IDLE_TIMEOUT", null, now);
            await RevokeSessionTokensAsync(session.Id, "IDLE_TIMEOUT", now, ct);
            await db.SaveChangesAsync(ct);
            throw new AuthException(401, "AUTH_TOKEN_EXPIRED", "Phiên đăng nhập đã hết hạn do không hoạt động. Vui lòng đăng nhập lại.");
        }
        var remainingDays = Math.Max(1, (int)Math.Ceiling((session.ExpiresAt - now).TotalDays));
        var (newToken, newEntity) = NewRefreshToken(session, token.FamilyId, token.Id, context, now, remainingDays);
        newEntity.ExpiresAt = session.ExpiresAt;
        token.UsedAt = now;
        token.ReplacedById = newEntity.Id;
        session.LastActivityAt = now;
        db.RefreshTokens.Add(newEntity);

        await WriteLoginLogAsync(user.TenantId, user.Id, user.Username, "TOKEN_REFRESH", null, session.Id, context, ct);

        var access = tokenService.CreateAccessToken(ToSubject(user, session.Id), TimeSpan.FromMinutes(policy.AccessTokenMinutes));
        var profile = await GetCurrentUserAsync(user.Id, ct);
        return new AuthResponse(access.Token, access.ExpiresAt, newToken, newEntity.ExpiresAt, user.MustChangePassword, profile);
    }

    public async Task LogoutAsync(Guid userId, Guid sessionId, RequestContext context, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var session = await db.UserSessions.SingleOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct);
        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        RevokeSession(session, "LOGOUT", userId, now);
        await RevokeSessionTokensAsync(session.Id, "LOGOUT", now, ct);
        await WriteLoginLogAsync(session.TenantId, userId, null, "LOGOUT", null, session.Id, context, ct);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new AuthException(401, "AUTH_UNAUTHORIZED", "Tài khoản không tồn tại.");

        var tenant = user.TenantId is null
            ? null
            : await db.Tenants.AsNoTracking().Where(t => t.Id == user.TenantId).Select(t => new { t.Code, t.Name }).SingleOrDefaultAsync(ct);

        var roles = await permissionService.GetActiveRolesAsync(userId, ct);
        var permissions = await permissionService.GetPermissionsAsync(userId, ct);

        var roleCodes = roles.Select(r => r.Code).ToList();
        if (user.IsPlatformAdmin)
        {
            roleCodes.Insert(0, "PLATFORM_ADMIN");
        }
        else if (user.IsTenantAdmin)
        {
            roleCodes.Insert(0, "TENANT_ADMIN");
        }

        return new CurrentUserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.DisplayName,
            user.Email,
            user.JobTitle,
            user.TenantId,
            tenant?.Code,
            tenant?.Name,
            user.UserType,
            user.IsPlatformAdmin,
            user.IsTenantAdmin,
            user.LastLoginAt,
            roleCodes,
            permissions.OrderBy(p => p).ToList());
    }

    public async Task ChangePasswordAsync(Guid userId, Guid sessionId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new AuthException(401, "AUTH_UNAUTHORIZED", "Tài khoản không tồn tại.");

        if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            throw new AuthException(400, "VALIDATION_FAILED", "Vui lòng nhập đầy đủ thông tin.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new AuthException(400, "VALIDATION_FAILED", "Mật khẩu xác nhận không khớp.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw new AuthException(400, "VALIDATION_FAILED", "Mật khẩu hiện tại không đúng.");
        }

        var policy = await GetPolicyAsync(user.TenantId, ct);
        var violations = ValidatePassword(request.NewPassword, policy);
        if (violations.Count > 0)
        {
            throw new AuthException(400, "VALIDATION_FAILED", "Mật khẩu mới chưa đạt yêu cầu: " + string.Join(", ", violations) + ".");
        }

        var recentHashes = await db.PasswordHistories.AsNoTracking()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(Math.Max(0, (int)policy.PasswordHistoryCount))
            .Select(h => h.PasswordHash)
            .ToListAsync(ct);
        if (passwordHasher.Verify(user.PasswordHash, request.NewPassword) || recentHashes.Any(h => passwordHasher.Verify(h, request.NewPassword)))
        {
            throw new AuthException(400, "VALIDATION_FAILED", $"Không được dùng lại {Math.Max(1, (int)policy.PasswordHistoryCount)} mật khẩu gần nhất.");
        }

        var now = DateTimeOffset.UtcNow;
        db.PasswordHistories.Add(new PasswordHistory { UserId = userId, PasswordHash = user.PasswordHash, CreatedAt = now });
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.PasswordChangedAt = now;
        user.MustChangePassword = false;
        user.SecurityStamp = Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));

        // Đăng xuất mọi phiên khác
        var otherSessions = await db.UserSessions
            .Where(s => s.UserId == userId && s.Id != sessionId && s.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var s in otherSessions)
        {
            RevokeSession(s, "PASSWORD_CHANGED", userId, now);
            await RevokeSessionTokensAsync(s.Id, "PASSWORD_CHANGED", now, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Phiên còn hiệu lực và tài khoản còn hoạt động — gọi cho mọi request có access token,
    /// nên đăng xuất / khóa tài khoản / thu hồi phiên có tác dụng ngay, không chờ access token hết hạn.
    /// </summary>
    public Task<bool> IsSessionActiveAsync(Guid sessionId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        return db.UserSessions.AsNoTracking()
            .Where(s => s.Id == sessionId && s.RevokedAt == null && s.ExpiresAt > now)
            .Join(db.Users.Where(u => u.Status == "ACTIVE"), s => s.UserId, u => u.Id, (s, u) => s.Id)
            .AnyAsync(ct);
    }

    // ------------------------------------------------------------------ helpers

    private async Task<User?> FindUserAsync(string login, string? tenantCode, CancellationToken ct)
    {
        // username / email là citext -> so sánh không phân biệt hoa thường ngay trong PostgreSQL
        var query = db.Users.Where(u => u.Username == login || u.Email == login);

        if (!string.IsNullOrEmpty(tenantCode))
        {
            var code = tenantCode.ToLowerInvariant();
            var tenantId = await db.Tenants.Where(t => t.Code.ToLower() == code).Select(t => (Guid?)t.Id).SingleOrDefaultAsync(ct);
            if (tenantId is null)
            {
                return null;
            }

            return await query.SingleOrDefaultAsync(u => u.TenantId == tenantId, ct);
        }

        // Ưu tiên tài khoản Platform Admin (tenant_id NULL) lên đầu
        var candidates = await query.OrderBy(u => u.TenantId != null).ThenBy(u => u.Id).Take(3).ToListAsync(ct);
        if (candidates.Count <= 1)
        {
            return candidates.SingleOrDefault();
        }

        var platformUser = candidates.SingleOrDefault(u => u.TenantId == null);
        return platformUser
            ?? throw new AuthException(400, "VALIDATION_FAILED", "Tài khoản tồn tại ở nhiều đơn vị, vui lòng nhập mã đơn vị.");
    }

    private async Task<SecurityPolicy> GetPolicyAsync(Guid? tenantId, CancellationToken ct)
    {
        return await db.SecurityPolicies.AsNoTracking()
                   .Where(p => p.TenantId == tenantId || p.TenantId == null)
                   .OrderBy(p => p.TenantId == null)
                   .FirstOrDefaultAsync(ct)
               ?? new SecurityPolicy();
    }

    private static List<string> ValidatePassword(string password, SecurityPolicy policy)
    {
        var errors = new List<string>();
        if (password.Length < policy.PasswordMinLength)
        {
            errors.Add($"tối thiểu {policy.PasswordMinLength} ký tự");
        }

        if (policy.PasswordRequireUpper && !password.Any(char.IsUpper))
        {
            errors.Add("có chữ hoa");
        }

        if (policy.PasswordRequireLower && !password.Any(char.IsLower))
        {
            errors.Add("có chữ thường");
        }

        if (policy.PasswordRequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("có chữ số");
        }

        if (policy.PasswordRequireSpecial && password.All(char.IsLetterOrDigit))
        {
            errors.Add("có ký tự đặc biệt");
        }

        return errors;
    }

    private (string Token, RefreshToken Entity) NewRefreshToken(
        UserSession session, Guid familyId, Guid? parentId, RequestContext context, DateTimeOffset now, int days)
    {
        var (token, hash) = tokenService.CreateRefreshToken();
        var entity = new RefreshToken
        {
            SessionId = session.Id,
            UserId = session.UserId,
            ClientId = session.ClientId,
            TokenHash = hash,
            FamilyId = familyId,
            ParentId = parentId,
            IssuedAt = now,
            ExpiresAt = now.AddDays(days),
            CreatedIp = ParseIp(context.IpAddress),
        };
        return (token, entity);
    }

    private async Task RevokeSessionTokensAsync(Guid sessionId, string reason, DateTimeOffset now, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens.Where(t => t.SessionId == sessionId && t.RevokedAt == null).ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.RevokedAt = now;
            t.RevokedReason = reason;
        }
    }

    private static void RevokeSession(UserSession session, string reason, Guid? by, DateTimeOffset now)
    {
        if (session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = now;
        session.RevokedReason = reason;
        session.RevokedBy = by;
    }

    /// <summary>
    /// Ghi audit.login_logs rồi lưu toàn bộ thay đổi đang chờ (kể cả khi sắp ném lỗi đăng nhập).
    /// </summary>
    private async Task WriteLoginLogAsync(
        Guid? tenantId, Guid? userId, string? username, string eventType, string? failureReason,
        Guid? sessionId, RequestContext context, CancellationToken ct)
    {
        db.LoginLogs.Add(new LoginLog
        {
            TenantId = tenantId,
            UserId = userId,
            UsernameAttempted = Truncate(username, 255),
            EventType = eventType,
            FailureReason = failureReason,
            AuthMethod = "PASSWORD",
            ClientId = "core-admin-web",
            SessionId = sessionId,
            IpAddress = ParseIp(context.IpAddress),
            UserAgent = Truncate(context.UserAgent, 1000),
            OccurredAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(ct);
    }

    private static AccessTokenSubject ToSubject(User user, Guid sessionId) =>
        new(user.Id, sessionId, user.TenantId, user.Username, user.FullName, user.IsPlatformAdmin, user.IsTenantAdmin);

    private static IPAddress? ParseIp(string? ip) => IPAddress.TryParse(ip, out var address) ? address : null;

    private static string? Truncate(string? value, int max) =>
        value is null ? null : value.Length <= max ? value : value[..max];
}
