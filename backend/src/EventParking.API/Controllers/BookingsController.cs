using System.Security.Claims;
using EventParking.API.DTOs.Bookings;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetMine(
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        if (customerId is null)
        {
            return Unauthorized();
        }

        var bookings =
            await _bookingService.GetForCustomerAsync(
                customerId,
                cancellationToken);

        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        if (customerId is null)
        {
            return Unauthorized();
        }

        var booking =
            await _bookingService.GetByIdAsync(
                id,
                customerId,
                cancellationToken);

        return booking is null
            ? NotFound()
            : Ok(booking);
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
        var customerId = GetCustomerId();

        if (customerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var booking =
                await _bookingService.CreateAsync(
                    request,
                    customerId,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = booking.Id },
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

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(
        typeof(BookingResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        if (customerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var booking =
                await _bookingService.CancelAsync(
                    id,
                    customerId,
                    cancellationToken);

            return Ok(booking);
        }
        catch (KeyNotFoundException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Booking not found.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Cancellation conflict.",
                detail: exception.Message);
        }
    }

    private string? GetCustomerId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }
}
