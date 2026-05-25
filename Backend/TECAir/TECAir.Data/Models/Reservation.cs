namespace TECAir.Data.Models
{
    /// <summary>
    /// Defines the transactional billing states of a reservation.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>The reservation was created, but payment has not been authorized or processed yet.</summary>
        Pending,

        /// <summary>The payment was successfully settled and cleared.</summary>
        Paid,

        /// <summary>The payment authorization timed out or was explicitly declined by the gateway.</summary>
        Failed,

        /// <summary>The booking was cancelled, and funds were returned to the user.</summary>
        Refunded
    }

    /// <summary>
    /// Represents a commercial booking transaction bound to a specific user and flight itinerary.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Gets or sets the unique alphanumeric reservation locator reference code.
        /// </summary>
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formal timestamp marking when the booking entry was registered.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the current billing transactional status of the booking ledger.
        /// </summary>
        public PaymentStatus PaymentState { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Gets or sets the foreign key identifier targeting the specific <see cref="User"/> account owner.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key alphanumeric reference code pointing to the assigned <see cref="Flight"/>.
        /// </summary>
        public string FlightNumber { get; set; } = string.Empty;
    }
}
