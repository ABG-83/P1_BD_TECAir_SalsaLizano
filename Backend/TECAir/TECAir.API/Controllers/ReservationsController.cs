using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Reservations;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    /// <summary>
    /// Exposes distributed endpoints to securely manage, create, consult, modify, and cancel flight reservations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController(IReservationService reservationService) : ControllerBase
    {
        private readonly IReservationService _reservationService = reservationService;

        /// <summary>
        /// Registers a new travel reservation itinerary entry inside the system ledger.
        /// </summary>
        /// <param name="dto">The client intake data container specifying flight tracking information and passenger identity.</param>
        /// <returns>A structured read-only data view of the newly generated booking entity configuration.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReservationResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _reservationService.CreateReservationAsync(dto);
            return CreatedAtAction(nameof(GetByCode), new { code = response.ReservationCode }, response);
        }

        /// <summary>
        /// Consults a specific reservation entry directly using its distinct unique alphanumeric code string.
        /// </summary>
        /// <param name="code">The unique token reference lookup code pointing to a target booking.</param>
        /// <returns>The detailed structural tracking file belonging to the target reservation.</returns>
        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReservationResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _reservationService.GetReservationByCodeAsync(code);
            if (result == null) return NotFound(new { message = $"Reservation file with tracker reference code '{code}' was not found." });
            return Ok(result);
        }

        /// <summary>
        /// Consults all historical flight bookings registered under a specific numeric passenger passport owner ID.
        /// </summary>
        /// <param name="userId">The system reference identifier matching a localized profile.</param>
        /// <returns>The collection of matching reservation files found across the tracking databases.</returns>
        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ReservationResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var results = await _reservationService.GetReservationsByUserIdAsync(userId);
            return Ok(results);
        }

        /// <summary>
        /// Modifies the active data properties linked to an existing ticket reservation tracking number securely.
        /// </summary>
        /// <param name="code">The alphanumeric unique locator tracking id identifying the target record to update.</param>
        /// <param name="dto">The data container carrying the newly modified context state elements.</param>
        [HttpPut("{code}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string code, [FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _reservationService.ModifyReservationAsync(code, dto);
            if (!success) return BadRequest(new { message = "The system was unable to commit modifications to the database structure layer." });

            return NoContent();
        }

        /// <summary>
        /// Cancels an active reservation entry from the system schedules, executing core business rejection workflows.
        /// </summary>
        /// <param name="code">The tracking token locator reference requested for immediate cancellation removal processing.</param>
        [HttpDelete("{code}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cancel(string code)
        {
            var success = await _reservationService.CancelReservationAsync(code);
            if (!success) return BadRequest(new { message = "The platform operation failed to transition the reservation target entry state." });

            return NoContent();
        }
    }
}
