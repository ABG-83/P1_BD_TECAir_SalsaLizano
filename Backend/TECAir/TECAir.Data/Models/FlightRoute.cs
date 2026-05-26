// =============================================================================
// File    : FlightRoute.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents an intermediate stop in a flight itinerary.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents an intermediate stop in a flight itinerary.
    /// </summary>
    public class FlightRoute
    {
        /// <summary>
        /// Gets or sets the flight number that owns the stop.
        /// </summary>
        public string FlightNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the airport identifier for the stop.
        /// </summary>
        public int AirportId { get; set; }

        /// <summary>
        /// Gets or sets the order of the stop in the itinerary.
        /// </summary>
        public int StopOrder { get; set; }
    }
}