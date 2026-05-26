// =============================================================================
// File    : PaymentsController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for payments operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Payments;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    /// <summary>
    /// API Controller routing all entry points associated with credit card transaction settlements.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PaymentController"/> utilizing constructor injection.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        /// <summary>
        /// Processes an incoming credit card authorization request to settle a pending reservation tracking slot.
        /// </summary>
        /// <param name="dto">The validated payload data transfer object transfer container containing payment details.</param>
        /// <returns>A standard structural action result tracking status code metrics.</returns>
        /// <response code="200">Returns if the transaction clearance was successfully authorized and updated in storage.</response>
        /// <response code="400">Returns if the card metadata format layout violates input validation constraints or booking is invalid.</response>
        [HttpPost("process")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
        {
            // The [ApiController] attribute implicitly runs an evaluation interceptor over the ModelState.
            // If any Data Annotation inside 'ProcessPaymentDto' (like [CreditCard] or [Required]) fails,
            // the pipeline halts immediately and replies with a clean 400 BadRequest, skipping this method body completely.

            // Delegate operational payment clearance logic downstream to the core domain service block
            bool paymentSettled = await _paymentService.ProcessReservationPaymentAsync(dto);

            if (!paymentSettled)
            {
                return BadRequest(new { Message = "The financial transaction clearance process failed to authorize near the gateway." });
            }

            return Ok(new { Message = "Payment successfully captured and authorized. Booking status is now finalized as 'Paid'." });
        }
    }
}
