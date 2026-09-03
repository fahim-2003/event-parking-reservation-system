using System.Security.Claims;
using EventParking.API.DTOs.Customers;
using EventParking.API.Identity;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet("me")]
    [Authorize(Roles = AppRoles.Customer)]
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

        return profile is null
            ? NotFound()
            : Ok(profile);
    }

    [HttpPut("me")]
    [Authorize(Roles = AppRoles.Customer)]
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

        return profile is null
            ? NotFound()
            : Ok(profile);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? search)
    {
        var customers =
            await _customerService.SearchCustomersAsync(search);

        return Ok(customers);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> GetCustomer(
        string id)
    {
        var customer =
            await _customerService.GetCustomerForAdminAsync(id);

        return customer is null
            ? NotFound()
            : Ok(customer);
    }
}