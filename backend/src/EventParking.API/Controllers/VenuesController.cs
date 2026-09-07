using EventParking.API.DTOs.Venues;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/venues")]
public sealed class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VenueResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VenueResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var venues = await _venueService.GetAllAsync(cancellationToken);

        return Ok(venues);
    }

    [HttpGet("{venueId:int}")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VenueResponse>> GetById(
        int venueId,
        CancellationToken cancellationToken)
    {
        var venue = await _venueService.GetByIdAsync(
            venueId,
            cancellationToken);

        if (venue is null)
        {
            return NotFound();
        }

        return Ok(venue);
    }
}