// =============================================================================
// Archivo  : CreateFlightDto.cs
// Capa     : TECAir.Core → DTOs/Flights
// Propósito: Define el JSON que el cliente envía al POST /api/flights
//            para registrar un nuevo vuelo con su ruta completa.
//            Incluye origen, escalas intermedias (en orden) y destino.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Flights
{
    public class CreateFlightDto
    {
        // Número de vuelo asignado manualmente. Ej: "TA-205"
        [Required]
        [MaxLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureTime { get; set; }

        // Debe ser posterior a DepartureTime (validado en el servicio)
        [Required]
        public DateTime ArrivalTime { get; set; }

        // Matrícula del avión asignado. Debe existir en la tabla AVION.
        [Required]
        [MaxLength(20)]
        public string AirplanePlateNumber { get; set; } = string.Empty;

        // ID del aeropuerto de origen
        [Required]
        [Range(1, int.MaxValue)]
        public int OriginAirportId { get; set; }

        // ID del aeropuerto de destino
        [Required]
        [Range(1, int.MaxValue)]
        public int DestinationAirportId { get; set; }

        // IDs de aeropuertos de escala EN ORDEN de visita.
        // Ejemplo: vuelo SJO → PTY → BOG → LIM → StopAirportIds = [PTY_id, BOG_id]
        // Lista vacía si es un vuelo directo sin escalas.
        public List<int> StopAirportIds { get; set; } = new();
    }
}