using System.Security.Claims;
using EventParking.API.DTOs.Bookings;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/customer/bookings")]
[Authorize(Roles = "Customer")]
public sealed class CustomerBookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public CustomerBookingsController(
        IBookingService bookingService)
    {
        _bookingService = bookingService;
    }


    [HttpPost]
    [ProducesResponseType(
        typeof(BookingResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(customerId))
        {
            return Unauthorized();
        }

        try
        {
            var booking = await _bookingService.CreateAsync(
                request,
                customerId,
                cancellationToken);

            return CreatedAtAction(
                nameof(Create),
                new { id = booking.Id },
                booking);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Booking conflict.",
                detail: exception.Message);
        }
    }
}
