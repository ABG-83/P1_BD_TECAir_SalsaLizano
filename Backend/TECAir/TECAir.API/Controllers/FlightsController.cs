// =============================================================================
// File    : FlightsController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for flights operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Flights;
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

        // GET /api/flights
        // Returns all flights with their full enriched route.
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FlightResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            return Ok(flights);
        }

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
        // GET /api/flights/{number}
        // Returns a specific flight with its route and stops. Returns 404 when it does not exist.
        [HttpGet("{number}")]
        [ProducesResponseType(typeof(FlightResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNumber(string number)
        {
            var flight = await _flightService.GetFlightByNumberAsync(number);

            if (flight is null)
                return NotFound(new { message = $"No se encontró el vuelo '{number}'." });

            return Ok(flight);
        }

        // POST /api/flights
        // Registers a new flight with its full route.
        // Example body (flight with one stopover):
        // {
        //   "flightNumber":        "TA-210",
        //   "departureTime":       "2026-07-01T08:00:00",
        //   "arrivalTime":         "2026-07-01T15:00:00",
        //   "airplanePlateNumber": "TEC-001",
        //   "originAirportId":     1,
        //   "destinationAirportId":4,
        //   "stopAirportIds":      [5]
        // }
        // For a direct flight: "stopAirportIds": []
        // PATCH /api/flights/{number}/status
        // Updates only the operational status of a flight.
        // Accepts: { "status": "abierto" | "cerrado" | "cancelado" | "programado" }
        [HttpPatch("{number}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(string number, [FromBody] UpdateFlightStatusDto dto)
        {
            await _flightService.UpdateFlightStatusAsync(number, dto.Status);
            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateFlightDto dto)
        {
            // [ApiController] validates the DTO [Required] fields automatically before the action executes.
            await _flightService.CreateFlightAsync(dto);

            // 201 Created pointing to the newly created flight.
            return CreatedAtAction(
                actionName: nameof(GetByNumber),
                routeValues: new { number = dto.FlightNumber },
                value: new { flightNumber = dto.FlightNumber }
            );
        }
    }
}
