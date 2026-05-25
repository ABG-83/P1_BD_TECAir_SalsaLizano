// =============================================================================
// Archivo  : CheckInResponseDto.cs
// Capa     : TECAir.Core → DTOs → CheckIn
// Propósito: DTO de respuesta que retorna el sistema tras completar un check-in.
//            Contiene la confirmación del registro y los datos necesarios para
//            que el frontend pueda mostrar o solicitar el pase de abordar.
// =============================================================================

namespace TECAir.Core.DTOs.CheckIn
{
    // Confirmación enviada al cliente tras un check-in exitoso
    public class CheckInResponseDto
    {
        // ID del registro de check-in recién creado en la BD
        public int CheckInId { get; set; }

        // Asiento asignado al pasajero (ej. "12A")
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje asignada (ej. "A1")
        public string BoardingGate { get; set; } = string.Empty;

        // Momento exacto en que se realizó el check-in y se generó el pase
        public DateTime PrintTime { get; set; }

        // ID de la reservación a la que pertenece este check-in
        public int ReservationId { get; set; }

        // Número de vuelo al que aplica el check-in
        public string FlightNumber { get; set; } = string.Empty;

        // Nombre completo del pasajero — útil para confirmar al funcionario
        public string PassengerName { get; set; } = string.Empty;
    }
}