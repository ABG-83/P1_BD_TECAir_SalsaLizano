// =============================================================================
// File    : CheckInDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.CheckIn
{
    // Data sent by the staff member to begin the check-in process.
    public class CheckInDto
    {
        // Passenger reservation ID to check in.
        // Must correspond to an existing reservation with payment_status = 'paid'.
        [Required(ErrorMessage = "El ID de la reservación es obligatorio.")]
        public string ReservationCode { get; set; } = string.Empty;

        // Asiento de preferencia seleccionado por el pasajero (ej. "12A", "5B")
        // The seat is validated to ensure it is not already occupied on the same flight.
        [Required(ErrorMessage = "El asiento es obligatorio.")]
        [MaxLength(10, ErrorMessage = "El asiento no puede superar los 10 caracteres.")]
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje asignada por el funcionario del aeropuerto (ej. "A1", "B3")
        [Required(ErrorMessage = "La puerta de abordaje es obligatoria.")]
        [MaxLength(10, ErrorMessage = "La puerta no puede superar los 10 caracteres.")]
        public string BoardingGate { get; set; } = string.Empty;
    }
}
