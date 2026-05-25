using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing customer flight bookings.
    /// </summary>
    public interface IReservationRepository
    {
        /// <summary>
        /// Registers a new reservation ledger entry and returns the code.
        /// </summary>
        Task<string> CreateAsync(Reservation reservation);

        /// <summary>
        /// Retrieves a single reservation details matching the unique reference code.
        /// </summary>
        Task<Reservation?> GetByCodeAsync(string reservationCode);

        /// <summary>
        /// Retrieves all historical reservation entries associated with a specific user.
        /// </summary>
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Updates an existing reservation's payment status or booking date details.
        /// </summary>
        Task<bool> UpdateAsync(Reservation reservation);

        /// <summary>
        /// Cancels a booking by changing its state or removing the transaction record.
        /// </summary>
        Task<bool> CancelAsync(string reservationCode);
    }
}
