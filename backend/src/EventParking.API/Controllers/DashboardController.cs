using System.Security.Claims;
using EventParking.API.DTOs.Dashboard;
using EventParking.API.Interfaces.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }


    [HttpGet("admin")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AdminDashboardResponse>> GetAdminDashboard(
        CancellationToken cancellationToken)
    {
        var result =
            await _dashboardService.GetAdminDashboardAsync(
                cancellationToken);

        return Ok(result);
    }


    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<CustomerDashboardResponse>> GetCustomerDashboard(
        CancellationToken cancellationToken)
    {
        var customerId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        var result =
            await _dashboardService.GetCustomerDashboardAsync(
                customerId,
                cancellationToken);

        return Ok(result);
    }
}
