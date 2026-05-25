// =============================================================================
// Archivo  : CheckInController.cs
// Capa     : TECAir.API → Controllers
// Propósito: Endpoints REST del Issue #15 — API de chequeo de pasajeros.
//            Permite a los funcionarios del aeropuerto registrar el check-in
//            de un pasajero, asignarle asiento y obtener el pase de abordar.
//            No contiene lógica de negocio ni SQL; delega al servicio.
//
//            Endpoints expuestos:
//              POST /api/checkin                          → realizar check-in
//              GET  /api/checkin/{id}                     → consultar check-in por ID
//              GET  /api/checkin/{id}/boarding-pass       → obtener pase de abordar
//              GET  /api/checkin/flight/{flightNumber}    → check-ins de un vuelo
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.BoardingPass;
using TECAir.Core.DTOs.CheckIn;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/checkin")]
    public class CheckInController(ICheckInService checkInService) : ControllerBase
    {
        private readonly ICheckInService _checkInService = checkInService;

        // POST /api/checkin
        // Registra el check-in de un pasajero: valida la reservación, asigna el
        // asiento solicitado y genera el registro del pase de abordar.
        // Body de ejemplo:
        // {
        //   "reservationId": 1,
        //   "seat": "12A",
        //   "boardingGate": "B3"
        // }
        [HttpPost]
        [ProducesResponseType(typeof(CheckInResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
        {
            // [ApiController] valida los [Required] del DTO antes de llegar aquí
            var result = await _checkInService.CheckInPassengerAsync(dto);

            // 201 Created apuntando al check-in recién creado
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.CheckInId },
                value: result
            );
        }

        // GET /api/checkin/{id}
        // Retorna los datos de un check-in específico por su ID.
        // Devuelve 404 si el check-in no existe.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CheckInResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var checkIn = await _checkInService.GetByIdAsync(id);

            if (checkIn is null)
                return NotFound(new { message = $"No existe un check-in con ID {id}." });

            return Ok(checkIn);
        }

        // GET /api/checkin/{id}/boarding-pass
        // Genera y retorna el pase de abordar completo para un check-in existente.
        // El pase incluye: puerta de abordaje, hora de salida, asiento y número de vuelo
        // (campos obligatorios según la especificación del proyecto).
        // Devuelve 404 si el check-in no existe.
        [HttpGet("{id:int}/boarding-pass")]
        [ProducesResponseType(typeof(BoardingPassDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBoardingPass(int id)
        {
            var boardingPass = await _checkInService.GetBoardingPassAsync(id);

            if (boardingPass is null)
                return NotFound(new { message = $"No se encontró el pase de abordar para el check-in con ID {id}." });

            return Ok(boardingPass);
        }

        // GET /api/checkin/flight/{flightNumber}
        // Retorna todos los check-ins registrados para un vuelo.
        // Permite al funcionario ver en tiempo real qué pasajeros ya abordaron.
        [HttpGet("flight/{flightNumber}")]
        [ProducesResponseType(typeof(IEnumerable<CheckInResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByFlight(string flightNumber)
        {
            var checkIns = await _checkInService.GetByFlightNumberAsync(flightNumber);
            return Ok(checkIns);
        }
    }
}