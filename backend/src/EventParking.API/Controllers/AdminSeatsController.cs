using EventParking.API.DTOs.Seats;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/admin/events/{eventId:int}/seats")]
[Authorize(Roles = "Administrator")]
public sealed class AdminSeatsController : ControllerBase
{
    private readonly ISeatService _seatService;

    public AdminSeatsController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(IReadOnlyList<SeatResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<SeatResponse>>> Generate(
        int eventId,
        [FromBody] GenerateSeatMapRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var seats = await _seatService.GenerateAsync(
                eventId,
                request,
                cancellationToken);

            return Created(
                $"/api/events/{eventId}/seats",
                seats);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid seat layout.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Seat layout conflict.",
                detail: exception.Message);
        }
    }

    [HttpPut("{seatId:int}")]
    [ProducesResponseType(
        typeof(SeatResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SeatResponse>> Update(
        int eventId,
        int seatId,
        [FromBody] UpdateSeatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var seat = await _seatService.UpdateAsync(
                eventId,
                seatId,
                request,
                cancellationToken);

            if (seat is null)
            {
                return NotFound();
            }

            return Ok(seat);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid seat.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Seat conflict.",
                detail: exception.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Seat concurrency conflict.",
                detail:
                    "The seat was changed by another request. Refresh and try again.");
        }
    }
}
