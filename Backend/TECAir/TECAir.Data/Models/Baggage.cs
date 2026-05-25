// =============================================================================
// Archivo  : Baggage.cs
// Capa     : TECAir.Data → Models
// Propósito: Modelo de dominio que representa una maleta asignada a un pasajero
//            chequeado. Mapea directamente a la tabla 'baggages'.
//
//            Regla de cobro por maletas:
//              1ra maleta → gratis
//              2da maleta → $50 adicionales
//              3ra maleta en adelante → $75 adicionales cada una
// =============================================================================

namespace TECAir.Data.Models
{
    // Representa una maleta registrada y asignada a la reservación de un pasajero
    public class Baggage
    {
        // Identificador único auto-generado por la base de datos
        public int BaggageId { get; set; }

        // Peso de la maleta en kilogramos
        public decimal Weight { get; set; }

        // Color de la maleta para identificación visual
        public string Color { get; set; } = string.Empty;

        // Llave foránea a la reservación del pasajero dueño de la maleta
        public int ReservationId { get; set; }

        // Llave foránea al usuario dueño de la maleta
        public int UserId { get; set; }
    }
}