using EventParking.API.DTOs.Seats;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/seats")]
public sealed class SeatsController : ControllerBase
{
    private readonly ISeatService _seatService;

    public SeatsController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<SeatResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SeatResponse>>> GetByEvent(
        int eventId,
        CancellationToken cancellationToken)
    {
        var seats = await _seatService.GetByEventAsync(
            eventId,
            cancellationToken);

        return Ok(seats);
    }
}
