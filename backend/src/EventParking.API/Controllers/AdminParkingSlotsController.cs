using EventParking.API.DTOs.Parking;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/admin/events/{eventId:int}/parking-slots")]
[Authorize(Roles = "Administrator")]
public sealed class AdminParkingSlotsController : ControllerBase
{
    private readonly IParkingService _parkingService;

    public AdminParkingSlotsController(IParkingService parkingService)
    {
        _parkingService = parkingService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(IReadOnlyList<ParkingSlotResponse>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<ParkingSlotResponse>>> Generate(
        int eventId,
        [FromBody] GenerateParkingLayoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var parkingSlots = await _parkingService.GenerateAsync(
                eventId,
                request,
                cancellationToken);

            return Created(
                $"/api/events/{eventId}/parking-slots",
                parkingSlots);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid parking layout.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Parking layout conflict.",
                detail: exception.Message);
        }
    }

    [HttpPut("{parkingSlotId:int}")]
    [ProducesResponseType(
        typeof(ParkingSlotResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingSlotResponse>> Update(
        int eventId,
        int parkingSlotId,
        [FromBody] UpdateParkingSlotRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var parkingSlot = await _parkingService.UpdateAsync(
                eventId,
                parkingSlotId,
                request,
                cancellationToken);

            if (parkingSlot is null)
            {
                return NotFound();
            }

            return Ok(parkingSlot);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid parking slot.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Parking slot conflict.",
                detail: exception.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Parking concurrency conflict.",
                detail:
                    "The parking slot was changed by another request. Refresh and try again.");
        }
    }

    [HttpDelete("{parkingSlotId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int eventId,
        int parkingSlotId,
        [FromQuery] byte[] rowVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _parkingService.DeleteAsync(
                eventId,
                parkingSlotId,
                rowVersion,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid parking slot.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Parking slot conflict.",
                detail: exception.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Parking concurrency conflict.",
                detail:
                    "The parking slot was changed by another request. Refresh and try again.");
        }
    }
}
