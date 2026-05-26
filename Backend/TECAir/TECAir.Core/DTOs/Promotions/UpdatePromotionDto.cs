// =============================================================================
// File    : UpdatePromotionDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
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

        // Must be later than StartDate and validated by the service.
        [Required]
        public DateOnly EndDate { get; set; }

        // Send null to remove the current promotion image.
        [MaxLength(300)]
        public string? Image { get; set; }

        // Allows the promotion to be activated or deactivated during editing.
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
