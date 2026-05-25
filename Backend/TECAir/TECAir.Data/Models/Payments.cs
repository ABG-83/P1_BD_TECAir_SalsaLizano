namespace TECAir.Data.Models
{
    /// <summary>
    /// Represents an immutable transactional ledger entry recording completed financial settlements for travel bookings.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the unique primary key tracking index for this financial receipt record.
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the target foreign key reference pointing to the associated reservation locator code.
        /// </summary>
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the absolute monetary value settled during the credit card transaction processing window.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the high-precision server timestamp noting exactly when the transaction cleared the acquiring gateway.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the unique transaction locator trace ID returned by the external financial gateway processor.
        /// </summary>
        /// <remarks>
        /// Crucial for cross-referencing ledger reports with external banking auditing teams.
        /// </remarks>
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the structural execution state string of the transaction (e.g., Completed, Failed, Refunded).
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}
