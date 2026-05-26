// =============================================================================
// File    : Payments.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a payment ledger entry related to a reservation.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents a payment ledger entry linked to a reservation.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the payment record.
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the reservation locator code associated with the payment.
        /// </summary>
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the amount charged for the payment.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the payment transaction was recorded.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the external transaction reference returned by the payment gateway.
        /// </summary>
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current payment status for the transaction.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}
