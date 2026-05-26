// =============================================================================
// File    : Baggage.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a checked baggage record assigned to a reservation and passenger.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents a baggage record assigned to a reservation.
    /// </summary>
    public class Baggage
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the baggage record.
        /// </summary>
        public int BaggageId { get; set; }

        /// <summary>
        /// Gets or sets the weight of the baggage item in kilograms.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Gets or sets the baggage color used for visual identification.
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reservation locator code that owns the baggage.
        /// </summary>
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user identifier associated with the baggage owner.
        /// </summary>
        public int UserId { get; set; }
    }
}
