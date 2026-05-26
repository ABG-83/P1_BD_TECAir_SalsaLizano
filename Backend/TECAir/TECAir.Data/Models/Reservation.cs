// =============================================================================
// File    : Reservation.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a booking reservation, its payment state, and its associated flight.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Defines the billing states available for a reservation.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>The reservation exists but payment has not yet been processed.</summary>
        Pending,

        /// <summary>The payment was successfully settled.</summary>
        Paid,

        /// <summary>The payment was rejected or could not be completed.</summary>
        Failed,

        /// <summary>The reservation was cancelled and funds were returned.</summary>
        Refunded
    }

    /// <summary>
    /// Represents a commercial booking transaction bound to a user and flight itinerary.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Gets or sets the unique reservation locator code.
        /// </summary>
        public string ReservationCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the reservation was created.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the current payment state of the reservation.
        /// </summary>
        public PaymentStatus PaymentState { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Gets or sets the identifier of the user who owns the reservation.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the flight number assigned to the reservation.
        /// </summary>
        public string FlightNumber { get; set; } = string.Empty;
    }
}
