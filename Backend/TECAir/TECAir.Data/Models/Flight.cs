namespace TECAir.Data.Models
{
    /// <summary>
    /// Defines the clear operational lifecycle states of a flight.
    /// </summary>
    public enum FlightStatus
    {
        /// <summary>The flight is created and scheduled, but boarding has not started.</summary>
        Scheduled,

        /// <summary>Passengers are currently boarding the aircraft.</summary>
        Boarding,

        /// <summary>The flight is delayed beyond its original departure timeline.</summary>
        Delayed,

        /// <summary>The aircraft has departed and is currently in transit.</summary>
        InAir,

        /// <summary>The flight arrived safely at its destination airport.</summary>
        Landed,

        /// <summary>The flight has been officially called off.</summary>
        Cancelled
    }

    /// <summary>
    /// Represents a flight scheduled or executed within the TECAir system.
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Gets or sets the unique alphanumeric flight number designation (e.g., "TA-204").
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
        /// Gets or sets the foreign key referencing the unique registration plate number of the assigned airplane.
        /// </summary>
        public string AirplanePlateNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the foreign key referencing the origin <see cref="Airport"/> identifier.
        /// </summary>
        public int OriginAirportId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the destination <see cref="Airport"/> identifier.
        /// </summary>
        public int DestinationAirportId { get; set; }
    }
}
