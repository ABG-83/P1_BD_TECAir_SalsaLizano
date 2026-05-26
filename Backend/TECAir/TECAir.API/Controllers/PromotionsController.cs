// =============================================================================
// File    : PromotionsController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for promotions operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Promotions;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController(IPromotionService promotionService) : ControllerBase
    {
        private readonly IPromotionService _promotionService = promotionService;

        // Returns all promotions, active and inactive, for the admin view.
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PromotionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return Ok(promotions);
        }

        // Returns only active promotions for the client promotions area.
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<PromotionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            return Ok(promotions);
        }

        // Finds a promotion by ID; returns 404 when it does not exist.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PromotionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);

            if (promotion is null)
                return NotFound(new { message = $"No se encontró una promoción con ID {id}." });

            return Ok(promotion);
        }

        // Registers a new promotion; returns 201 with the created resource location.
        // Example body:
        // {
        //   "price": 199.99,
        //   "startDate": "2026-06-01",
        //   "endDate": "2026-08-31",
        //   "image": "img/promo_sjo_mia.jpg",   ← optional, can be omitted
        //   "originAirportId": 1,
        //   "destinationAirportId": 8
        // }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePromotionDto dto)
        {
            // [ApiController] validates the DTO [Required] fields before the action executes.
            var newId = await _promotionService.CreatePromotionAsync(dto);

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = newId },
                value: new { promotionId = newId }
            );
        }

        // Updates all fields of an existing promotion; returns 204 when the operation succeeds.
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePromotionDto dto)
        {
            await _promotionService.UpdatePromotionAsync(id, dto);
            return NoContent();
        }

        // Deletes a promotion permanently; returns 204 when the operation succeeds and 404 when the promotion does not exist.
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _promotionService.DeletePromotionAsync(id);
            return NoContent();
        }
    }
}
