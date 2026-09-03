using EventParking.API.DTOs.Parking;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/parking-slots")]
public sealed class ParkingSlotsController : ControllerBase
{
    private readonly IParkingService _parkingService;

    public ParkingSlotsController(IParkingService parkingService)
    {
        _parkingService = parkingService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ParkingSlotResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotResponse>>> GetByEvent(
        int eventId,
        CancellationToken cancellationToken)
    {
        var parkingSlots = await _parkingService.GetByEventAsync(
            eventId,
            cancellationToken);

        return Ok(parkingSlots);
    }
}
