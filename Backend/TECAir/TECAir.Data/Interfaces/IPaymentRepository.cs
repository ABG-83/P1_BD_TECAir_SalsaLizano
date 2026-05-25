using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Provides low-level database structural capabilities to read and write payment traces.
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Commits an immutable ledger record tracking an approved payment directly into storage.
        /// </summary>
        Task<int> CreateAsync(Payment payment);

        /// <summary>
        /// Pulls a singular financial log matching its key index parameter.
        /// </summary>
        Task<Payment?> GetByIdAsync(int paymentId);

        /// <summary>
        /// Gathers all payment tracking documents linked to an isolated reservation code lookup token.
        /// </summary>
        Task<IEnumerable<Payment>> GetByReservationCodeAsync(string reservationCode);
    }
}
