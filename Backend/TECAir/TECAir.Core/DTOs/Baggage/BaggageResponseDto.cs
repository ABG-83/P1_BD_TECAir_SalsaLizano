// =============================================================================
// Archivo  : BaggageResponseDto.cs
// Capa     : TECAir.Core → DTOs/Baggage
// Propósito: DTO de respuesta para operaciones con maletas.
//            Incluye los datos de la maleta más la información de cobro calculada
//            según las reglas del enunciado:
//              - 1ra maleta : $0.00
//              - 2da maleta : $50.00
//              - 3ra en adelante : $75.00 cada una
// =============================================================================

namespace TECAir.Core.DTOs.Baggage
{
    public class BaggageResponseDto
    {
        // Identificador único generado por la BD
        public int BaggageId { get; set; }
        public decimal Weight { get; set; }
        public string Color { get; set; } = string.Empty;
        public string ReservationCode { get; set; } = string.Empty;

        // Nombre completo del pasajero dueño (resuelto desde reservación → usuario)
        public string PassengerName { get; set; } = string.Empty;

        // Posición de esta maleta para el pasajero (1ra, 2da, 3ra…)
        public int BaggagePosition { get; set; }

        // Cargo adicional únicamente por esta maleta según la tarifa del enunciado
        public decimal ExtraChargeForThisBag { get; set; }

        // Suma acumulada de todos los cargos adicionales del pasajero
        public decimal TotalExtraCharge { get; set; }
    }
}
