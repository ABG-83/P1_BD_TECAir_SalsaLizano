// =============================================================================
// File    : BaggageController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for baggage operations.
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
        // Registers a baggage item and assigns it to the passenger for the specified reservation.
        // Validates that the passenger has already checked in before accepting the baggage item.
        // Example body:
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
            // [ApiController] validates the DTO [Required] fields before the action executes.
            var result = await _baggageService.AddBaggageAsync(dto);

            // 201 Created pointing to the newly registered baggage item.
            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = result.BaggageId },
                value: result
            );
        }

        // GET /api/baggage/{id}
        // Returns the baggage data together with its individual charge.
        // Returns 404 when the baggage item does not exist.
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
        // Returns all baggage items for a passenger with the full charge breakdown.
        // Allows the staff member to review the total accumulated additional charges.
        [HttpGet("reservation/{reservationCode}")]
        [ProducesResponseType(typeof(IEnumerable<BaggageResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByReservation(string reservationCode)
        {
            var baggages = await _baggageService.GetByReservationCodeAsync(reservationCode);
            return Ok(baggages);
        }
    }
}
