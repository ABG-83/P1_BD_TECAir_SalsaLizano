// =============================================================================
// Archivo  : UpdatePromotionDto.cs
// Capa     : TECAir.Core → DTOs/Promotions
// Propósito: Define el cuerpo JSON que el cliente envía al PUT /api/promotions/{id}
//            para editar una promoción existente. Incluye IsActive para que el
//            administrador pueda activar o desactivar la promoción sin necesitar
//            un endpoint separado.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Promotions
{
    public class UpdatePromotionDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Price { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        // Debe ser posterior a StartDate, validado en el servicio
        [Required]
        public DateOnly EndDate { get; set; }

        // Enviar null para eliminar la imagen actual de la promoción
        [MaxLength(300)]
        public string? Image { get; set; }

        // Permite activar o desactivar la promoción durante la edición
        [Required]
        public bool IsActive { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OriginAirportId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DestinationAirportId { get; set; }
    }
}