// =============================================================================
// Archivo  : FlightClosingController.cs
// Capa     : TECAir.API → Controllers
// Propósito: Endpoints REST del Issue #30 — Cierre de vuelos.
//            Permite a los funcionarios del aeropuerto cerrar un vuelo y obtener
//            la lista oficial final de pasajeros con check-in y sus maletas.
//            No contiene lógica de negocio; delega completamente al servicio.
//
//            Endpoints expuestos:
//              PUT  /api/flight-closing/{flightNumber}/close      → cierra el vuelo
//              GET  /api/flight-closing/{flightNumber}/final-list → consulta la lista final
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
        // Solo funciona si el vuelo está en estado 'Boarding'.
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
        // Retorna la lista oficial de pasajeros con check-in sin cambiar el estado del vuelo.
        // Útil para revisar quiénes tienen check-in antes de confirmar el cierre.
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