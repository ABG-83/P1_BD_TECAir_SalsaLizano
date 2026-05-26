// =============================================================================
// File    : Promotion.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a price promotion between two airports with a defined validity period.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents a price promotion between two airports.
    /// </summary>
    public class Promotion
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the promotion.
        /// </summary>
        public int PromotionId { get; set; }

        /// <summary>
        /// Gets or sets the promotional price for the route.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the date when the promotion becomes available.
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date for the promotion.
        /// </summary>
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// Gets or sets the optional image path or URL associated with the promotion.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the promotion is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the origin airport identifier for the promotion.
        /// </summary>
        public int OriginAirportId { get; set; }

        /// <summary>
        /// Gets or sets the destination airport identifier for the promotion.
        /// </summary>
        public int DestinationAirportId { get; set; }

        public string? OriginAirportName { get; set; }
        public string? DestinationAirportName { get; set; }
    }
}