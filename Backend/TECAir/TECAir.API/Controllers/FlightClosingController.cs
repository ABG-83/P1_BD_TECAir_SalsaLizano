// =============================================================================
// File    : FlightClosingController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for flightclosing operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/flight-closing")]
    public class FlightClosingController(IFlightClosingService flightClosingService) : ControllerBase
    {
        private readonly IFlightClosingService _flightClosingService = flightClosingService;

        // PUT /api/flight-closing/{flightNumber}/close
        // Cambia el estado del vuelo a 'InAir' y retorna la lista oficial de pasajeros con check-in.
        // Only works when the flight is in the 'Boarding' state.
        [HttpPut("{flightNumber}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CloseFlight(string flightNumber)
        {
            var finalList = await _flightClosingService.CloseFlightAsync(flightNumber);
            return Ok(finalList);
        }

        // GET /api/flight-closing/{flightNumber}/final-list
        // Returns the official passenger list with check-in without changing the flight status.
        // Useful for reviewing who has checked in before confirming the closure.
        [HttpGet("{flightNumber}/final-list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFinalList(string flightNumber)
        {
            var finalList = await _flightClosingService.GetFinalListAsync(flightNumber);
            return Ok(finalList);
        }
    }
}
