// =============================================================================
// File    : BoardingPassDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.BoardingPass
{
    // All information printed on the boarding pass
    public class BoardingPassDto
    {
        // ── Datos del vuelo ────────────────────────────────────────────────────

        // Flight number (for example, "TA-003")
        public string FlightNumber { get; set; } = string.Empty;

        // Aeropuerto de origen del vuelo (nombre completo)
        public string OriginAirport { get; set; } = string.Empty;

        // Aeropuerto de destino del vuelo (nombre completo)
        public string DestinationAirport { get; set; } = string.Empty;

        // Departure date and time of the flight — required field according to the specification
        public DateTime DepartureTime { get; set; }

        // Fecha y hora estimada de llegada
        public DateTime ArrivalTime { get; set; }

        // ── Datos del pasajero ─────────────────────────────────────────────────

        // Full name of the passenger as stored in the system
        public string PassengerName { get; set; } = string.Empty;

        // Passenger email address (useful for digital boarding pass delivery)
        public string PassengerEmail { get; set; } = string.Empty;

        // ── Datos del check-in ─────────────────────────────────────────────────

        // Assigned seat for the passenger — required field according to the specification
        public string Seat { get; set; } = string.Empty;

        // Boarding gate — required field according to the specification
        public string BoardingGate { get; set; } = string.Empty;

        // Timestamp when the boarding pass was generated or printed
        public DateTime PrintTime { get; set; }

        // Check-in ID that generated this boarding pass (useful for traceability)
        public int CheckInId { get; set; }
    }
}
