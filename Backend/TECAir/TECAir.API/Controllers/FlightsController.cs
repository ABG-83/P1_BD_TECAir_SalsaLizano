using Microsoft.AspNetCore.Mvc;
using TECAir.Core.Interfaces;
using TECAir.Data.Models;

namespace TECAir.API.Controllers
{
    /// <summary>
    /// Exposes API endpoints for searching and managing flight information.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController(IFlightService flightService) : ControllerBase
    {
        private readonly IFlightService _flightService = flightService;

        /// <summary>
        /// Searches available flights executing a specific route between two airports.
        /// </summary>
        /// <param name="originId">The identifier of the origin airport.</param>
        /// <param name="destinationId">The identifier of the destination airport.</param>
        /// <returns>A list of matching flights meeting the search criteria.</returns>
        /// <response code="200">Returns the collection of flights matching the route filter.</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Flight>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByRoute([FromQuery] int originId, [FromQuery] int destinationId)
        {
            var flights = await _flightService.SearchFlightsByRouteAsync(originId, destinationId);
            return Ok(flights);
        }
    }
}
