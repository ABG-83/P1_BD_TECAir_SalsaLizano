// =============================================================================
// File    : CheckIn.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a passenger check-in record for a flight and its boarding metadata.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents a check-in record for a passenger on a flight.
    /// </summary>
    public class CheckIn
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the check-in record.
        /// </summary>
        public int CheckInId { get; set; }

        /// <summary>
        /// Gets or sets the assigned seat for the passenger, such as "12A".
        /// </summary>
        public string Seat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the boarding gate assigned to the passenger.
        /// </summary>
        public string BoardingGate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the boarding pass was generated.
        /// </summary>
        public DateTime PrintTime { get; set; }

        /// <summary>
        /// Gets or sets the reservation locator code that owns the check-in.
        /// </summary>
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the flight number associated with the check-in.
        /// </summary>
        public string FlightNumber { get; set; } = string.Empty;
    }
}