// =============================================================================
// File    : Flight.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a flight schedule and its current operational status within TECAir.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Defines the operational lifecycle states of a flight.
    /// </summary>
    public enum FlightStatus
    {
        /// <summary>The flight is scheduled and not yet boarding.</summary>
        Scheduled,

        /// <summary>Passengers are currently boarding the aircraft.</summary>
        Boarding,

        /// <summary>The flight has been delayed beyond its original departure time.</summary>
        Delayed,

        /// <summary>The aircraft has departed and is currently in transit.</summary>
        InAir,

        /// <summary>The flight arrived safely at its destination airport.</summary>
        Landed,

        /// <summary>The flight has been cancelled.</summary>
        Cancelled
    }

    /// <summary>
    /// Represents a flight scheduled or executed within the TECAir system.
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Gets or sets the unique alphanumeric flight number designation.
        /// </summary>
        public string FlightNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scheduled departure date and time.
        /// </summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Gets or sets the scheduled arrival date and time.
        /// </summary>
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Gets or sets the current operational status of the flight.
        /// </summary>
        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        /// <summary>
        /// Gets or sets the registration plate number of the assigned airplane.
        /// </summary>
        public string AirplanePlateNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the origin airport identifier.
        /// </summary>
        public int OriginAirportId { get; set; }

        /// <summary>
        /// Gets or sets the destination airport identifier.
        /// </summary>
        public int DestinationAirportId { get; set; }

        public string OriginAirportName { get; set; } = string.Empty;

        public string DestinationAirportName { get; set; } = string.Empty;
    }
}
