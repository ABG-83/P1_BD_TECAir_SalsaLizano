// =============================================================================
// Archivo  : FlightClosingDto.cs
// Capa     : TECAir.Core → DTOs/FlightClosing
// Propósito: Define el JSON que retorna el endpoint de cierre de vuelo.
//            Contiene la lista oficial y final de pasajeros que efectivamente
//            viajarán, basada en quiénes completaron el check-in antes del cierre.
// =============================================================================

namespace TECAir.Core.DTOs.FlightClosing
{
    // Respuesta completa del cierre de un vuelo con la lista oficial de pasajeros
    public class FlightClosingDto
    {
        // Número de vuelo cerrado
        public string FlightNumber { get; set; } = string.Empty;

        // Nuevo estado del vuelo después del cierre (siempre 'InAir')
        public string Status { get; set; } = string.Empty;

        // Hora de salida programada del vuelo
        public DateTime DepartureTime { get; set; }

        // Hora de llegada programada del vuelo
        public DateTime ArrivalTime { get; set; }

        // Total oficial de pasajeros que viajarán (solo los que hicieron check-in)
        public int TotalPassengers { get; set; }

        // Total de maletas de todos los pasajeros con check-in
        public int TotalBaggages { get; set; }

        // Lista detallada de cada pasajero con check-in confirmado
        public List<CheckedInPassengerDto> Passengers { get; set; } = [];
    }

    // Representa a un pasajero con check-in dentro de la lista oficial del vuelo
    public class CheckedInPassengerDto
    {
        // ID del check-in del pasajero
        public int CheckInId { get; set; }

        // Asiento asignado al pasajero (ej. "12A")
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje asignada (ej. "A1")
        public string BoardingGate { get; set; } = string.Empty;

        // Nombre completo del pasajero
        public string FullName { get; set; } = string.Empty;

        // Correo electrónico de contacto del pasajero
        public string Email { get; set; } = string.Empty;

        // Cantidad de maletas registradas para este pasajero
        public int BaggageCount { get; set; }

        // Cargo adicional por maletas extra según la regla de negocio:
        // 1ra gratis, 2da $50, 3ra en adelante $75 c/u
        public decimal BaggageSurcharge { get; set; }
    }
}