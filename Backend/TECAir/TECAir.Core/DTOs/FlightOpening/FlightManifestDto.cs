// =============================================================================
// Archivo  : FlightManifestDto.cs
// Capa     : TECAir.Core → DTOs/FlightOpening
// Propósito: Define el JSON que retorna el endpoint de apertura de vuelo.
//            Contiene el resumen del vuelo y la lista de pasajeros confirmados
//            con su conteo de maletas para facilitar la carga y balance del avión.
// =============================================================================

namespace TECAir.Core.DTOs.FlightOpening
{
    // Respuesta completa del manifiesto de apertura de un vuelo
    public class FlightManifestDto
    {
        // Número de vuelo abierto
        public string FlightNumber { get; set; } = string.Empty;

        // Nuevo estado del vuelo después de la apertura (siempre 'Boarding')
        public string Status { get; set; } = string.Empty;

        // Hora de salida programada del vuelo
        public DateTime DepartureTime { get; set; }

        // Hora de llegada programada del vuelo
        public DateTime ArrivalTime { get; set; }

        // Total de pasajeros confirmados (reservaciones pagadas)
        public int TotalPassengers { get; set; }

        // Total de maletas de todos los pasajeros confirmados
        public int TotalBaggages { get; set; }

        // Lista detallada de cada pasajero confirmado con su equipaje
        public List<PassengerManifestItemDto> Passengers { get; set; } = [];
    }

    // Representa a un pasajero individual dentro del manifiesto del vuelo
    public class PassengerManifestItemDto
    {
        // ID de la reservación del pasajero
        public int ReservationId { get; set; }

        // Nombre completo del pasajero
        public string FullName { get; set; } = string.Empty;

        // Correo electrónico de contacto del pasajero
        public string Email { get; set; } = string.Empty;

        // Cantidad de maletas registradas para esta reservación
        public int BaggageCount { get; set; }

        // Cargo adicional por maletas extra según la regla de negocio:
        // 1ra gratis, 2da $50, 3ra en adelante $75 c/u
        public decimal BaggageSurcharge { get; set; }
    }
}