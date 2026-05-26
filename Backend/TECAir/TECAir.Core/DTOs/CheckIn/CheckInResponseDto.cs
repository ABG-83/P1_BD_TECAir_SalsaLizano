// =============================================================================
// File    : CheckInResponseDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.CheckIn
{
    // Confirmation sent to the customer after a successful check-in
    public class CheckInResponseDto
    {
        // ID of the newly created check-in record in the database
        public int CheckInId { get; set; }

        // Asiento asignado al pasajero (ej. "12A")
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje asignada (ej. "A1")
        public string BoardingGate { get; set; } = string.Empty;

        // Exact timestamp when the check-in was completed and the boarding pass was generated
        public DateTime PrintTime { get; set; }

        // Reservation ID associated with this check-in
        public string ReservationCode { get; set; } = string.Empty;

        // Flight number to which this check-in applies
        public string FlightNumber { get; set; } = string.Empty;

        // Passenger full name — useful to confirm it for the staff member
        public string PassengerName { get; set; } = string.Empty;
    }
}
