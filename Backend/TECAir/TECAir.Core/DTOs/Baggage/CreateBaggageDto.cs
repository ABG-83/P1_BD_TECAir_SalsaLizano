// =============================================================================
// Archivo  : CreateBaggageDto.cs 
// Capa     : TECAir.Core → DTOs/Baggage
// Propósito: DTO de entrada para registrar y asignar una maleta a un pasajero.
//            Solo requiere el reservation_id del pasajero (el user_id se resuelve
//            internamente desde la reservación). El servicio valida que el
//            pasajero ya haya hecho check-in antes de registrar la maleta.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Baggage
{
    public class BaggageDto
    {
        // ID de la reservación del pasajero dueño de la maleta
        [Required]
        public int ReservationId { get; set; }

        // Peso de la maleta en kilogramos (mínimo 0.1 kg)
        [Required]
        [Range(0.1, 999.9)]
        public decimal Weight { get; set; }

        // Color descriptivo para identificación visual en bodega
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Color { get; set; } = string.Empty;
    }
}