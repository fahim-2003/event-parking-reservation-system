using Microsoft.EntityFrameworkCore;
using EventParking.API.DTOs.Venues;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/admin/venues")]
[Authorize(Roles = "Administrator")]
public sealed class AdminVenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public AdminVenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VenueResponse>> Create(
        [FromBody] CreateVenueRequest request,
        CancellationToken cancellationToken)
    {
        var venue = await _venueService.CreateAsync(
            request,
            cancellationToken);

        return Created($"/api/venues/{venue.Id}", venue);
    }

    [HttpPut("{venueId:int}")]
    [ProducesResponseType(typeof(VenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VenueResponse>> Update(
        int venueId,
        [FromBody] UpdateVenueRequest request,
        CancellationToken cancellationToken)
    {
        var venue = await _venueService.UpdateAsync(
            venueId,
            request,
            cancellationToken);

        if (venue is null)
        {
            return NotFound();
        }

        return Ok(venue);
    }

    [HttpDelete("{venueId:int}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<IActionResult> Delete(
    int venueId,
    CancellationToken cancellationToken)
{
    try
    {
        var deleted = await _venueService.DeleteAsync(
            venueId,
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
            title: "Venue conflict.",
            detail: "The venue cannot be deleted because it is referenced by existing data.");
    }
}
}