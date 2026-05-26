// =============================================================================
// File    : AirportsController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for airports operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.Interfaces;
using TECAir.Data.Models;

namespace TECAir.API.Controllers
{
    /// <summary>
    /// Exposes API endpoints for retrieving airport information to support flight selection.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AirportsController"/> class.
    /// </remarks>
    /// <param name="airportService">The business service layer handling airport logistics.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class AirportsController(IAirportService airportService) : ControllerBase
    {
        private readonly IAirportService _airportService = airportService;

        /// <summary>
        /// Retrieves all operational airports registered in the system.
        /// Useful for populating both Origin and Destination dropdown components in the React frontend.
        /// </summary>
        /// <response code="200">Returns the complete list of available airports.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Airport>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var airports = await _airportService.GetAllAirportsAsync();
            return Ok(airports);
        }

        /// <summary>
        /// Retrieves detailed specifications for a single airport using its database record identifier.
        /// </summary>
        /// <param name="id">The unique integer identifying the airport.</param>
        /// <response code="200">Returns the requested airport data structure.</response>
        /// <response code="404">If no airport matches the supplied criteria.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Airport), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var airport = await _airportService.GetAirportByIdAsync(id);

            if (airport == null)
            {
                return NotFound(new { message = $"Airport with ID {id} was not found in the system." });
            }

            return Ok(airport);
        }
    }
}
