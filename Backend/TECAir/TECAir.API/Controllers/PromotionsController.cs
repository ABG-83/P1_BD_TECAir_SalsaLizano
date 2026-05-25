// =============================================================================
// Archivo  : PromotionsController.cs
// Capa     : TECAir.API → Controllers
// Propósito: Endpoints REST del Issue #13 — API de gestión de promociones.
//            Expone operaciones CRUD sobre promociones de precio entre aeropuertos.
//            No contiene lógica de negocio ni SQL; delega completamente al servicio.
//
//            Endpoints expuestos:
//              GET    /api/promotions          → todas las promociones (admin)
//              GET    /api/promotions/active   → solo promociones activas (cliente)
//              GET    /api/promotions/{id}     → una promoción por ID
//              POST   /api/promotions          → registrar nueva promoción
//              PUT    /api/promotions/{id}     → editar promoción existente
//              DELETE /api/promotions/{id}     → eliminar promoción
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

        // Retorna todas las promociones (activas e inactivas) para la vista de administrador
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PromotionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return Ok(promotions);
        }

        // Retorna solo las promociones activas para el área de promociones de la vista cliente
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<PromotionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            return Ok(promotions);
        }

        // Busca una promoción por ID; retorna 404 si no existe
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

        // Registra una nueva promoción; retorna 201 con la ubicación del recurso creado
        // Body de ejemplo:
        // {
        //   "price": 199.99,
        //   "startDate": "2026-06-01",
        //   "endDate": "2026-08-31",
        //   "image": "img/promo_sjo_mia.jpg",   ← opcional, puede omitirse
        //   "originAirportId": 1,
        //   "destinationAirportId": 8
        // }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePromotionDto dto)
        {
            // [ApiController] valida los [Required] del DTO antes de llegar aquí
            var newId = await _promotionService.CreatePromotionAsync(dto);

            return CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { id = newId },
                value: new { promotionId = newId }
            );
        }

        // Edita todos los campos de una promoción existente; retorna 204 si fue exitoso
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePromotionDto dto)
        {
            await _promotionService.UpdatePromotionAsync(id, dto);
            return NoContent();
        }

        // Elimina permanentemente una promoción; retorna 204 si fue exitoso, 404 si no existe
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