using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Features.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Số liệu tổng quan cho màn hình Dashboard.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IDashboardService dashboard, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Tổng quan: thống kê tổ chức/người dùng, xu hướng đăng nhập, đăng nhập gần đây, phân hệ, tình trạng hệ thống.
    /// </summary>
    /// <param name="days">Số ngày của biểu đồ đăng nhập (1–90, mặc định 7).</param>
    [HttpGet("overview")]
    [ProducesResponseType<DashboardOverview>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardOverview>> Overview([FromQuery] int days = 7, CancellationToken ct = default) =>
        Ok(await dashboard.GetOverviewAsync(currentUser.TenantId, currentUser.IsPlatformAdmin, days, ct));
}
