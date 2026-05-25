using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Reservations
{
    /// <summary>
    /// Input contract carrying required customer data payload to start a new booking sequence.
    /// </summary>
    public class CreateReservationDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A valid positive User ID is strictly required.")]
        public int UserId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "A target flight tracking number must be assigned.")]
        public string FlightNumber { get; set; } = string.Empty;
    }
}
