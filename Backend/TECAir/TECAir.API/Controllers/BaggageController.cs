// =============================================================================
// Archivo  : BaggageController.cs
// Capa     : TECAir.API → Controllers
// Propósito: Endpoints REST del Issue #16 — API de maletas.
//            Permite a los funcionarios del aeropuerto registrar maletas y
//            asignarlas a pasajeros ya chequeados.
//            No contiene lógica de negocio ni SQL; delega al servicio.
//
//            Endpoints expuestos:
//              POST /api/baggage                              → registrar maleta
//              GET  /api/baggage/{id}                         → consultar maleta por ID
//              GET  /api/baggage/reservation/{reservationId}  → maletas de una reservación
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Baggage;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/baggage")]
    public class BaggageController(IBaggageService baggageService) : ControllerBase
    {
        private readonly IBaggageService _baggageService = baggageService;

        // POST /api/baggage
        // Registra una maleta y la asigna al pasajero de la reservación indicada.
        // Valida que el pasajero ya haya hecho check-in antes de aceptar la maleta.
        // Body de ejemplo:
        // {
        //   "reservationId": 5,
        //   "weight": 23.5,
        //   "color": "Negro"
        // }
        [HttpPost]
        [ProducesResponseType(typeof(BaggageResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddBaggage([FromBody] BaggageDto dto)
        {
            // [ApiController] valida los [Required] del DTO antes de llegar aquí
            var result = await _baggageService.AddBaggageAsync(dto);

            // 201 Created apuntando a la maleta recién registrada
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.BaggageId },
                value: result
            );
        }

        // GET /api/baggage/{id}
        // Retorna los datos de una maleta con su cobro individual.
        // Devuelve 404 si la maleta no existe.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BaggageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var baggage = await _baggageService.GetByIdAsync(id);

            if (baggage is null)
                return NotFound(new { message = $"No existe una maleta con ID {id}." });

            return Ok(baggage);
        }

        // GET /api/baggage/reservation/{reservationCode}
        // Retorna todas las maletas de un pasajero con el desglose completo de cobros.
        // Permite al funcionario ver el total acumulado de cargos adicionales.
        [HttpGet("reservation/{reservationCode}")]
        [ProducesResponseType(typeof(IEnumerable<BaggageResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByReservation(string reservationCode)
        {
            var baggages = await _baggageService.GetByReservationCodeAsync(reservationCode);
            return Ok(baggages);
        }
    }
}