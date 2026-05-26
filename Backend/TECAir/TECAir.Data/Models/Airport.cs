// =============================================================================
// File    : Airport.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents an airport facility and its location metadata within the TECAir system.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents an airport facility within the TECAir system.
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the airport.
        /// </summary>
        public int AirportId { get; set; }

        /// <summary>
        /// Gets or sets the official commercial name of the airport.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the geographic location of the airport.
        /// </summary>
        public string Location { get; set; } = string.Empty;
    }
}
