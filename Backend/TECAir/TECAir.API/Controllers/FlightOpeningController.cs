// =============================================================================
// Archivo  : FlightOpeningController.cs
// Capa     : TECAir.API → Controllers
// Propósito: Endpoints REST del Issue #29 — Apertura de vuelos.
//            Permite a los funcionarios del aeropuerto abrir un vuelo y consultar
//            el manifiesto de pasajeros confirmados con su conteo de maletas.
//            No contiene lógica de negocio; delega completamente al servicio.
//
//            Endpoints expuestos:
//              PUT  /api/flight-opening/{flightNumber}/open     → abre el vuelo
//              GET  /api/flight-opening/{flightNumber}/manifest → consulta el manifiesto
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
        // Cambia el estado del vuelo a 'Boarding' y retorna el manifiesto de pasajeros confirmados.
        // Solo funciona si el vuelo está en estado 'Scheduled'.
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
        // Retorna el manifiesto de pasajeros confirmados sin cambiar el estado del vuelo.
        // Útil para revisar quiénes viajarán antes de confirmar la apertura.
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