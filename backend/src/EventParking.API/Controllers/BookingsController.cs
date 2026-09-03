using EventParking.API.DTOs.Bookings;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/customer/bookings")]
[Authorize(Roles = "Customer")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Unauthorized();
        }

        try
        {
            var booking = await _bookingService.CreateAsync(
                request,
                customerId,
                cancellationToken);

            return Created(
                $"/api/customer/bookings/{booking.Id}",
                booking);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid booking.",
                detail: exception.Message);
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
