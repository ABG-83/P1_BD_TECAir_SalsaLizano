using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Flights
{
    public class UpdateFlightStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
