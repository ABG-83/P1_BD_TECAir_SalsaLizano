// =============================================================================
// Archivo  : CheckInDto.cs
// Capa     : TECAir.Core → DTOs → CheckIn
// Propósito: DTO de entrada para realizar el check-in de un pasajero.
//            Contiene los datos que el funcionario del aeropuerto envía
//            al endpoint POST /api/checkin.
//
//            El servicio usa estos datos para:
//              - Localizar la reservación del pasajero
//              - Asignar el asiento solicitado (si está disponible)
//              - Registrar la puerta de abordaje
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.CheckIn
{
    // Datos que el funcionario envía para iniciar el proceso de check-in
    public class CheckInDto
    {
        // ID de la reservación del pasajero a chequear
        // Debe corresponder a una reservación existente con payment_status = 'paid'
        [Required(ErrorMessage = "El ID de la reservación es obligatorio.")]
        public int ReservationId { get; set; }

        // Asiento de preferencia seleccionado por el pasajero (ej. "12A", "5B")
        // Se valida que no esté ya ocupado en el mismo vuelo
        [Required(ErrorMessage = "El asiento es obligatorio.")]
        [MaxLength(10, ErrorMessage = "El asiento no puede superar los 10 caracteres.")]
        public string Seat { get; set; } = string.Empty;

        // Puerta de abordaje asignada por el funcionario del aeropuerto (ej. "A1", "B3")
        [Required(ErrorMessage = "La puerta de abordaje es obligatoria.")]
        [MaxLength(10, ErrorMessage = "La puerta no puede superar los 10 caracteres.")]
        public string BoardingGate { get; set; } = string.Empty;
    }
}