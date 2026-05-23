// =============================================================================
// Archivo  : FlightResponseDto.cs
// Capa     : TECAir.Core → DTOs/Flights
// Propósito: Define el JSON que la API retorna al consultar un vuelo.
//            A diferencia del modelo Flight (que solo tiene IDs), este DTO
//            incluye nombres de aeropuertos y escalas enriquecidas para que
//            el frontend no necesite hacer llamadas adicionales.
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

        // Capacidad del avión asignado (útil para el frontend)
        public int PassengerCapacity { get; set; }

        // Aeropuerto de origen con nombre y ubicación
        public AirportSummaryDto Origin { get; set; } = new();

        // Aeropuerto de destino con nombre y ubicación
        public AirportSummaryDto Destination { get; set; } = new();

        // Escalas intermedias ordenadas (vacío si vuelo directo)
        public List<FlightStopDto> Stops { get; set; } = new();
    }

    // Datos resumidos de un aeropuerto dentro de la respuesta de vuelo
    public class AirportSummaryDto
    {
        public int AirportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    // Datos de una escala intermedia dentro de la respuesta de vuelo
    public class FlightStopDto
    {
        public int Order { get; set; }       // Posición en la ruta (1, 2, 3...)
        public int AirportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}