// =============================================================================
// File    : FlightResponseDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.Flights
{
    public class FlightResponseDto
    {
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AirplanePlateNumber { get; set; } = string.Empty;

        // Assigned aircraft capacity (useful for the frontend).
        public int PassengerCapacity { get; set; }

        // Flight price in USD.
        public decimal Price { get; set; }

        // Origin airport with name and location.
        public AirportSummaryDto Origin { get; set; } = new();

        // Destination airport with name and location.
        public AirportSummaryDto Destination { get; set; } = new();

        // Ordered intermediate stopovers (empty for a direct flight).
        public List<FlightStopDto> Stops { get; set; } = new();
    }

    // Brief airport data included in the flight response.
    public class AirportSummaryDto
    {
        public int AirportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    // Intermediate stop data included in the flight response.
    public class FlightStopDto
    {
        public int Order { get; set; }       // Position in the route (1, 2, 3...)
        public int AirportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
