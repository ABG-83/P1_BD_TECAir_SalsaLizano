// =============================================================================
// File    : IReservationRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for reservation records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for reservation records.
    /// </summary>
    public interface IReservationRepository
    {
        /// <summary>
        /// Gets all reservations in the system.
        /// </summary>
        Task<IEnumerable<Reservation>> GetAllAsync();

        /// <summary>
        /// Creates a reservation record and returns the generated reservation code.
        /// </summary>
        Task<string> CreateAsync(Reservation reservation);

        /// <summary>
        /// Gets a reservation by its reservation code, or returns null when no match exists.
        /// </summary>
        Task<Reservation?> GetByCodeAsync(string reservationCode);

        /// <summary>
        /// Gets all reservations associated with a user identifier.
        /// </summary>
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Searches reservations by partial user full name (case-insensitive).
        /// </summary>
        Task<IEnumerable<Reservation>> SearchByNameAsync(string name);

        /// <summary>
        /// Updates an existing reservation record.
        /// </summary>
        Task<bool> UpdateAsync(Reservation reservation);

        /// <summary>
        /// Cancels a reservation record.
        /// </summary>
        Task<bool> CancelAsync(string reservationCode);

        /// <summary>
        /// Gets paid reservations for a flight number.
        /// </summary>
        Task<IEnumerable<Reservation>> GetPaidByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Gets the active reservation count for a flight.
        /// </summary>
        Task<int> GetActiveCountByFlightNumberAsync(string flightNumber);
    }
}
