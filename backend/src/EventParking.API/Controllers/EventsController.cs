using EventParking.API.DTOs.Events;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<EventResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DateTime? dateFromUtc,
        [FromQuery] DateTime? dateToUtc,
        [FromQuery] int? venueId,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllAsync(
            search,
            dateFromUtc,
            dateToUtc,
            venueId,
            categoryId,
            cancellationToken);

        return Ok(events);
    }

    [HttpGet("{eventId:int}")]
    [ProducesResponseType(
        typeof(EventResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById(
        int eventId,
        CancellationToken cancellationToken)
    {
        var eventResponse = await _eventService.GetByIdAsync(
            eventId,
            cancellationToken);

        if (eventResponse is null)
        {
            return NotFound();
        }

        return Ok(eventResponse);
    }
}
