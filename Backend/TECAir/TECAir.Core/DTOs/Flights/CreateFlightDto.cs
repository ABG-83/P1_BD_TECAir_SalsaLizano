// =============================================================================
// File    : CreateFlightDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Flights
{
    public class CreateFlightDto
    {
        // Flight number assigned manually. Example: "TA-205"
        [Required]
        [MaxLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureTime { get; set; }

        // Must be later than DepartureTime (validated by the service).
        [Required]
        public DateTime ArrivalTime { get; set; }

        // Registered aircraft plate. It must exist in the AVION table.
        [Required]
        [MaxLength(20)]
        public string AirplanePlateNumber { get; set; } = string.Empty;

        // Origin airport ID.
        [Required]
        [Range(1, int.MaxValue)]
        public int OriginAirportId { get; set; }

        // Destination airport ID.
        [Required]
        [Range(1, int.MaxValue)]
        public int DestinationAirportId { get; set; }

        // Flight price in USD.
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        public decimal Price { get; set; }

        // Optional status override used only on updates (ignored on create).
        public string? Status { get; set; }

        // Ordered stopover airport IDs in visit order.
        // Example: SJO → PTY → BOG → LIM → StopAirportIds = [PTY_id, BOG_id]
        // Empty list for a direct flight without stopovers.
        public List<int> StopAirportIds { get; set; } = new();
    }
}
