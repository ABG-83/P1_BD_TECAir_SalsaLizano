// =============================================================================
// File    : FlightClosingDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.FlightClosing
{
    // Full response for closing a flight with the official passenger list.
    public class FlightClosingDto
    {
        // Closed flight number
        public string FlightNumber { get; set; } = string.Empty;

        // New flight status after closure (always 'InAir')
        public string Status { get; set; } = string.Empty;

        // Scheduled departure time of the flight.
        public DateTime DepartureTime { get; set; }

        // Scheduled arrival time of the flight.
        public DateTime ArrivalTime { get; set; }

        // Official total of passengers who will travel (only those who checked in)
        public int TotalPassengers { get; set; }

        // Total baggage count for all passengers with check-in
        public int TotalBaggages { get; set; }

        // Detailed list of every passenger with confirmed check-in.
        public List<CheckedInPassengerDto> Passengers { get; set; } = [];
    }

    // Represents a passenger with check-in inside the official flight list.
    public class CheckedInPassengerDto
    {
        // Passenger check-in ID.
        public int CheckInId { get; set; }

        // Assigned seat for the passenger (for example, "12A").
        public string Seat { get; set; } = string.Empty;

        // Assigned boarding gate (for example, "A1").
        public string BoardingGate { get; set; } = string.Empty;

        // Full name of the passenger.
        public string FullName { get; set; } = string.Empty;

        // Contact email address of the passenger.
        public string Email { get; set; } = string.Empty;

        // Number of baggage items registered for this passenger.
        public int BaggageCount { get; set; }

        // Additional charge for extra baggage items according to the business rule:
        // 1st is free, 2nd is $50, 3rd and onward is $75 each.
        public decimal BaggageSurcharge { get; set; }
    }
}
