using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Xác thực: đăng nhập, làm mới token, đăng xuất, thông tin tài khoản, đổi mật khẩu.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Tên policy giới hạn tần suất cho đăng nhập / làm mới token (cấu hình Auth:RateLimit:*).
    /// </summary>
    public const string RateLimitPolicy = "auth";

    /// <summary>
    /// Đăng nhập bằng tên đăng nhập/email + mật khẩu.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiError>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiError>(StatusCodes.Status423Locked)]
    [ProducesResponseType<ApiError>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        Ok(await auth.LoginAsync(request, RequestInfo(), ct));

    /// <summary>
    /// Đổi refresh token lấy access token mới (refresh token được xoay vòng).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiError>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct) =>
        Ok(await auth.RefreshAsync(request, RequestInfo(), ct));

    /// <summary>
    /// Đăng xuất: thu hồi phiên hiện tại và toàn bộ refresh token của phiên.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(currentUser.UserId!.Value, currentUser.SessionId!.Value, RequestInfo(), ct);
        return NoContent();
    }

    /// <summary>
    /// Thông tin tài khoản đang đăng nhập kèm vai trò và quyền.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct) =>
        Ok(await auth.GetCurrentUserAsync(currentUser.UserId!.Value, ct));

    /// <summary>
    /// Đổi mật khẩu; các phiên khác của tài khoản sẽ bị đăng xuất.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiError>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(currentUser.UserId!.Value, currentUser.SessionId!.Value, request, ct);
        return NoContent();
    }

    private RequestContext RequestInfo() =>
        new(HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString());
}

/// <summary>
/// Định dạng lỗi chuẩn trả về cho client.
/// </summary>
public sealed record ApiError(string Code, string Message);
