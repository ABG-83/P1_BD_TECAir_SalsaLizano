using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Reservations
{
    /// <summary>
    /// Data Transfer Object containing parameters required to allocate a pending travel booking manifest slot.
    /// </summary>
    public class CreateReservationDto
    {
        /// <summary>
        /// Gets or sets the numerical tracking reference index of the user creating the booking.
        /// </summary>
        [Required(ErrorMessage = "A valid User identity index ownership allocation parameter is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The owner User ID tracking index must be a positive integer value higher than zero.")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the distinct alphanumeric indicator identifier code of the targeted aircraft journey.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "The flight number identifier code is strictly required to hold a booking space.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Flight numbers must be between 2 and 20 characters length.")]
        public string FlightNumber { get; set; } = string.Empty;
    }
}
