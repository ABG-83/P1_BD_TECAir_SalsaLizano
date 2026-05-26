// =============================================================================
// File    : FlightManifestDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.FlightOpening
{
    // Full response for the flight opening manifest.
    public class FlightManifestDto
    {
        // Opened flight number
        public string FlightNumber { get; set; } = string.Empty;

        // New flight status after opening (always 'Boarding').
        public string Status { get; set; } = string.Empty;

        // Scheduled departure time of the flight.
        public DateTime DepartureTime { get; set; }

        // Scheduled arrival time of the flight.
        public DateTime ArrivalTime { get; set; }

        // Total confirmed passengers (paid reservations)
        public int TotalPassengers { get; set; }

        // Total baggage count for all confirmed passengers
        public int TotalBaggages { get; set; }

        // Detailed list of each confirmed passenger and their baggage.
        public List<PassengerManifestItemDto> Passengers { get; set; } = [];
    }

    // Represents an individual passenger inside the flight manifest.
    public class PassengerManifestItemDto
    {
        // Passenger reservation ID.
        public string ReservationCode { get; set; } = string.Empty;

        // Full name of the passenger.
        public string FullName { get; set; } = string.Empty;

        // Contact email address of the passenger.
        public string Email { get; set; } = string.Empty;

        // Number of baggage items registered for this reservation.
        public int BaggageCount { get; set; }

        // Additional charge for extra baggage items according to the business rule:
        // 1st is free, 2nd is $50, 3rd and onward is $75 each.
        public decimal BaggageSurcharge { get; set; }
    }
}
