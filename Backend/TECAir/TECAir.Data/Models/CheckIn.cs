// =============================================================================
// Archivo  : CheckIn.cs
// Capa     : TECAir.Data → Models
// Propósito: Modelo de dominio que representa el chequeo de un pasajero en un
//            vuelo. Mapea directamente a la tabla 'check_ins'.
//            Cada check-in genera el pase de abordar con asiento, puerta y hora.
//            La restricción UNIQUE sobre reservation_id garantiza que un
//            pasajero no pueda hacer check-in más de una vez por reservación.
// =============================================================================

namespace TECAir.Data.Models
{
    // Representa el registro de check-in de un pasajero para un vuelo específico
    public class CheckIn
    {
        // Identificador único auto-generado por la base de datos
        public int CheckInId { get; set; }

        // Asiento asignado al pasajero (ej. "12A", "5B")
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje asignada (ej. "A1", "B3")
        public string BoardingGate { get; set; } = string.Empty;

        // Fecha y hora en que se imprimió/generó el pase de abordar
        public DateTime PrintTime { get; set; }

        // Llave foránea a la reservación (UNIQUE: una sola por reservación)
        public int ReservationId { get; set; }

        // Llave foránea al vuelo al que pertenece este check-in
        public string FlightNumber { get; set; } = string.Empty;
    }
}