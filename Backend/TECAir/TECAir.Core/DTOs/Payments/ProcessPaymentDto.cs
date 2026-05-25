using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Payments
{
    /// <summary>
    /// Data Transfer Object representing the credit card settlement payload required to process a booking payment.
    /// </summary>
    public class ProcessPaymentDto
    {
        /// <summary>
        /// Gets or sets the target reservation alphanumeric identifier locator code.
        /// </summary>
        [Required(ErrorMessage = "The reservation code locator is strictly required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Reservation code must be between 3 and 50 characters.")]
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the monetary total value to be charged to the card account.
        /// </summary>
        [Required(ErrorMessage = "The payment amount is strictly required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The absolute transaction amount must be greater than zero.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the raw primary account credit card number string.
        /// </summary>
        [Required(ErrorMessage = "The credit card account number is required.")]
        [CreditCard(ErrorMessage = "The provided value is not a structurally valid credit card sequence format.")]
        public string CardNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the legal full name matching the credit card owner profile identity.
        /// </summary>
        [Required(ErrorMessage = "The cardholder owner name is strictly required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Cardholder name must be between 2 and 100 characters.")]
        public string CardholderName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target card expiration threshold tracking date following the strict MM/YY format.
        /// </summary>
        [Required(ErrorMessage = "The card expiration deadline date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "The expiration date must strictly align with the 'MM/YY' standard formatting rules.")]
        public string ExpirationDate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the 3 or 4-digit credit card security code sequence identifier.
        /// </summary>
        [Required(ErrorMessage = "The card verification value (CVV) safety code is required.")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "The CVV security tracking key must be exactly 3 or 4 numerical digits.")]
        public string Cvv { get; set; } = string.Empty;
    }
}
