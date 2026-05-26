// =============================================================================
// File    : ReservationResponseDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.Reservations
{
    /// <summary>
    /// Output read-only security view wrapper sent back across HTTP boundaries to downstream microservices or clients.
    /// </summary>
    public class ReservationResponseDto
    {
        public string ReservationCode { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PaymentState { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string? UserName { get; set; }
    }
}
