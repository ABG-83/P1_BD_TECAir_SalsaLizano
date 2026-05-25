using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Payments
{
    public class ProcessPaymentDto
    {
        [Required]
        public string ReservationCode { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [CreditCard]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        public string CardholderName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Expiration must be MM/YY format.")]
        public string ExpirationDate { get; set; } = string.Empty;

        [Required]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string Cvv { get; set; } = string.Empty;
    }
}
