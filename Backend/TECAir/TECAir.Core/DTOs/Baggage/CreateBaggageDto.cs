// =============================================================================
// File    : CreateBaggageDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Baggage
{
    public class BaggageDto
    {
        // Reservation ID of the passenger who owns the baggage item.
        [Required]
        public string ReservationCode { get; set; } = string.Empty;

        // Baggage weight in kilograms (minimum 0.1 kg).
        [Required]
        [Range(0.1, 999.9)]
        public decimal Weight { get; set; }

        // Descriptive color for visual identification in the hold.
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Color { get; set; } = string.Empty;
    }
}
