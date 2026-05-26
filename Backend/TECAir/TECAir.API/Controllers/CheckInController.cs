// =============================================================================
// File    : CheckInController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for checkin operations.
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
        // Registers a passenger check-in: validates the reservation and assigns the
        // requested seat and generates the boarding pass record.
        // Example body:
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
            // [ApiController] validates the DTO [Required] fields before the action executes.
            var result = await _checkInService.CheckInPassengerAsync(dto);

            // 201 Created pointing to the newly created check-in record.
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.CheckInId },
                value: result
            );
        }

        // GET /api/checkin/{id}
        // Returns the details of a specific check-in by its ID.
        // Returns 404 when the check-in does not exist.
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
        // Generates and returns the complete boarding pass for an existing check-in.
        // The boarding pass includes the boarding gate, departure time, seat, and flight number.
        // (required fields according to the project specification).
        // Returns 404 when the check-in does not exist.
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
        // Returns all check-ins recorded for a flight.
        // Allows the staff member to see in real time which passengers have boarded.
        [HttpGet("flight/{flightNumber}")]
        [ProducesResponseType(typeof(IEnumerable<CheckInResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByFlight(string flightNumber)
        {
            var checkIns = await _checkInService.GetByFlightNumberAsync(flightNumber);
            return Ok(checkIns);
        }
    }
}
