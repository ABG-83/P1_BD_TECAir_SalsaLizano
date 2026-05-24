// =============================================================================
// Archivo  : Reservation.cs
// Capa     : TECAir.Data → Models
// Propósito: Modelo de dominio que representa una reservación de un pasajero
//            en un vuelo. Mapea directamente a la tabla 'reservations'.
//            El estado de pago puede ser: pending, paid o cancelled.
// =============================================================================

namespace TECAir.Data.Models
{
    // Representa una reservación de pasajero para un vuelo específico
    public class Reservation
    {
        // Identificador único auto-generado por la base de datos
        public int ReservationId { get; set; }

        // Fecha y hora en que se realizó la reservación
        public DateTime Date { get; set; }

        // Estado del pago: 'pending', 'paid' o 'cancelled'
        public string PaymentStatus { get; set; } = "pending";

        // Llave foránea al usuario que realizó la reservación
        public int UserId { get; set; }

        // Llave foránea al vuelo reservado
        public string FlightNumber { get; set; } = string.Empty;
    }
}