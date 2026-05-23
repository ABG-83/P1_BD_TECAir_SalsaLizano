// =============================================================================
// Archivo  : Promotion.cs
// Capa     : TECAir.Data → Models
// Propósito: Modelo de dominio que representa una promoción de precio entre
//            dos aeropuertos. Mapea directamente a la tabla 'promotions' en
//            PostgreSQL. La imagen es opcional.
// =============================================================================

namespace TECAir.Data.Models
{
    // Representa una promoción de precio ofrecida entre dos aeropuertos por un período definido
    public class Promotion
    {
        // Identificador único auto-generado por la base de datos
        public int PromotionId { get; set; }

        // Precio promocional de la ruta, debe ser mayor a cero
        public decimal Price { get; set; }

        // Fecha en que la promoción empieza a estar disponible para los pasajeros
        public DateOnly StartDate { get; set; }

        // Fecha en que la promoción expira, debe ser posterior a StartDate
        public DateOnly EndDate { get; set; }

        // Ruta o URL de la imagen de la promoción, es null cuando no se asoció ninguna imagen
        public string? Image { get; set; }

        // Indica si la promoción está activa y visible para los pasajeros
        public bool IsActive { get; set; } = true;

        // Llave foránea al aeropuerto de origen
        public int OriginAirportId { get; set; }

        // Llave foránea al aeropuerto de destino
        public int DestinationAirportId { get; set; }
    }
}