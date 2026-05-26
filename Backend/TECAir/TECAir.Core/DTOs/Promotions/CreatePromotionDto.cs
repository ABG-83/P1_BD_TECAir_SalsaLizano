// =============================================================================
// File    : CreatePromotionDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Promotions
{
    public class CreatePromotionDto
    {
        // Promotional route price. Must be greater than 0.
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Price { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        // Must be later than StartDate and validated by the service.
        [Required]
        public DateOnly EndDate { get; set; }

        // Image path or URL. Optional field and can be omitted from the JSON payload.
        [MaxLength(300)]
        public string? Image { get; set; }

        // Origin airport ID for the promotion.
        [Required]
        [Range(1, int.MaxValue)]
        public int OriginAirportId { get; set; }

        // Destination airport ID for the promotion.
        [Required]
        [Range(1, int.MaxValue)]
        public int DestinationAirportId { get; set; }
    }
}
