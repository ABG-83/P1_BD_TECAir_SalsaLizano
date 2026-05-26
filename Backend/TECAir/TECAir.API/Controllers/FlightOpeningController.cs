// =============================================================================
// File    : FlightOpeningController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for flightopening operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/flight-opening")]
    public class FlightOpeningController(IFlightOpeningService flightOpeningService) : ControllerBase
    {
        private readonly IFlightOpeningService _flightOpeningService = flightOpeningService;

        // PUT /api/flight-opening/{flightNumber}/open
        // Changes the flight state to 'Boarding' and returns the confirmed passenger manifest.
        // Only works when the flight is in the 'Scheduled' state.
        [HttpPut("{flightNumber}/open")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OpenFlight(string flightNumber)
        {
            var manifest = await _flightOpeningService.OpenFlightAsync(flightNumber);
            return Ok(manifest);
        }

        // GET /api/flight-opening/{flightNumber}/manifest
        // Returns the confirmed passenger manifest without changing the flight status.
        // Useful for reviewing who will travel before confirming the opening.
        [HttpGet("{flightNumber}/manifest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetManifest(string flightNumber)
        {
            var manifest = await _flightOpeningService.GetManifestAsync(flightNumber);
            return Ok(manifest);
        }
    }
}
