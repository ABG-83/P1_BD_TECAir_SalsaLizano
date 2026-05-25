// =============================================================================
// Archivo  : BoardingPassDto.cs
// Capa     : TECAir.Core → DTOs → BoardingPass
// Propósito: DTO que representa el pase de abordar de un pasajero.
//            Contiene todos los campos que la especificación indica deben
//            aparecer en el pase: puerta de abordaje, hora de salida,
//            asiento y número de vuelo.
//
//            Este DTO es el que se usa para:
//              - Mostrar el pase en pantalla (vista web/móvil)
//              - Enviar el pase por correo electrónico
//              - Enviarlo a una impresora o dispositivo móvil
// =============================================================================

namespace TECAir.Core.DTOs.BoardingPass
{
    // Toda la información que aparece impresa en el pase de abordar
    public class BoardingPassDto
    {
        // ── Datos del vuelo ────────────────────────────────────────────────────

        // Número de vuelo (ej. "TA-003")
        public string FlightNumber { get; set; } = string.Empty;

        // Aeropuerto de origen del vuelo (nombre completo)
        public string OriginAirport { get; set; } = string.Empty;

        // Aeropuerto de destino del vuelo (nombre completo)
        public string DestinationAirport { get; set; } = string.Empty;

        // Fecha y hora de salida del vuelo — campo obligatorio según la especificación
        public DateTime DepartureTime { get; set; }

        // Fecha y hora estimada de llegada
        public DateTime ArrivalTime { get; set; }

        // ── Datos del pasajero ─────────────────────────────────────────────────

        // Nombre completo del pasajero tal como está registrado en el sistema
        public string PassengerName { get; set; } = string.Empty;

        // Correo electrónico del pasajero (útil para envío digital del pase)
        public string PassengerEmail { get; set; } = string.Empty;

        // ── Datos del check-in ─────────────────────────────────────────────────

        // Asiento asignado al pasajero — campo obligatorio según la especificación
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje — campo obligatorio según la especificación
        public string BoardingGate { get; set; } = string.Empty;

        // Momento en que se generó/imprimió el pase de abordar
        public DateTime PrintTime { get; set; }

        // ID del check-in que originó este pase (útil para trazabilidad)
        public int CheckInId { get; set; }
    }
}