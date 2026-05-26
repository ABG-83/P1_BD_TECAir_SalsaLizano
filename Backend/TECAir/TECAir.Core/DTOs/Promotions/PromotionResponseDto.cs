// =============================================================================
// File    : PromotionResponseDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.Promotions
{
    public class PromotionResponseDto
    {
        public int PromotionId { get; set; }
        public decimal Price { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // Null when the promotion has no associated image.
        public string? Image { get; set; }
        public bool IsActive { get; set; }

        // Enriched origin airport with name and location.
        public PromotionAirportDto Origin { get; set; } = new();

        // Enriched destination airport with name and location.
        public PromotionAirportDto Destination { get; set; } = new();
    }

    // Brief airport data included in the promotion response.
    public class PromotionAirportDto
    {
        public int AirportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
