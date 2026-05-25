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

        // Retorna solo las reservaciones con payment_status = 'paid' de un vuelo
        // Es el método central para la apertura de vuelos (pasajeros confirmados)
        Task<IEnumerable<Reservation>> GetPaidByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Queries the transactional ledger storage to count all active, non-cancelled bookings assigned to a specific flight identifier.
        /// </summary>
        /// <param name="flightNumber">The alphanumeric unique locator code of the target aircraft journey.</param>
        /// <returns>The total amount of currently occupied seats for the manifest evaluation.</returns>
        Task<int> GetActiveCountByFlightNumberAsync(string flightNumber);
    }
}
