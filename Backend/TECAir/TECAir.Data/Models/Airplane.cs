// =============================================================================
// File    : Airplane.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents an airplane in the TECAir fleet and stores its capacity metadata.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents an airplane in the TECAir fleet.
    /// </summary>
    public class Airplane
    {
        /// <summary>
        /// Gets or sets the unique registration plate number of the airplane.
        /// </summary>
        public string PlateNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total passenger capacity configured for the airplane.
        /// </summary>
        public int PassengerCapacity { get; set; }

        /// <summary>
        /// Gets or sets the total number of physical seats available on the airplane.
        /// </summary>
        public int SeatCount { get; set; }
    }
}