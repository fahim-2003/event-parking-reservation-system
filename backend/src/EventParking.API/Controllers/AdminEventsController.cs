using EventParking.API.DTOs.Events;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/admin/events")]
[Authorize(Roles = "Administrator")]
public sealed class AdminEventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public AdminEventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(EventResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventResponse>> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var eventResponse = await _eventService.CreateAsync(
                request,
                cancellationToken);

            return Created(
                $"/api/events/{eventResponse.Id}",
                eventResponse);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid event.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Event conflict.",
                detail: exception.Message);
        }
    }

    [HttpPut("{eventId:int}")]
    [ProducesResponseType(
        typeof(EventResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventResponse>> Update(
        int eventId,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var eventResponse = await _eventService.UpdateAsync(
                eventId,
                request,
                cancellationToken);

            if (eventResponse is null)
            {
                return NotFound();
            }

            return Ok(eventResponse);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid event.",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Event conflict.",
                detail: exception.Message);
        }
    }

    [HttpDelete("{eventId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int eventId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _eventService.DeleteAsync(
                eventId,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Event conflict.",
                detail:
                    "The event cannot be deleted because it is referenced by existing data.");
        }
    }
}
