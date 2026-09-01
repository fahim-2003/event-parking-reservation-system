using System.Security.Claims;
using EventParking.API.DTOs.Customers;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = AppRoles.Customer)]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("me")]
    [ProducesResponseType(
        typeof(CustomerProfileResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOwnProfile()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var profile =
            await _customerService.GetOwnProfileAsync(userId);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    [ProducesResponseType(
        typeof(CustomerProfileResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOwnProfile(
        [FromBody] UpdateCustomerProfileRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var profile =
            await _customerService.UpdateOwnProfileAsync(
                userId,
                request);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}