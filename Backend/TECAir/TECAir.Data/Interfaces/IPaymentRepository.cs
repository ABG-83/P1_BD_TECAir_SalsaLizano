// =============================================================================
// File    : IPaymentRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for payment records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for payment records.
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Creates a payment record and returns the generated identifier.
        /// </summary>
        Task<int> CreateAsync(Payment payment);

        /// <summary>
        /// Gets a payment record by its identifier, or returns null when no match exists.
        /// </summary>
        Task<Payment?> GetByIdAsync(int paymentId);

        /// <summary>
        /// Gets all payment records associated with a reservation code.
        /// </summary>
        Task<IEnumerable<Payment>> GetByReservationCodeAsync(string reservationCode);
    }
}
