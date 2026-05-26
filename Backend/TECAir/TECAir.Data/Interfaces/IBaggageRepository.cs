// =============================================================================
// File    : IBaggageRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for baggage records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for baggage records.
    /// </summary>
    public interface IBaggageRepository
    {
        /// <summary>
        /// Creates a baggage record and returns the generated identifier.
        /// </summary>
        Task<int> CreateAsync(Baggage baggage);

        /// <summary>
        /// Gets all baggage records associated with a reservation code.
        /// </summary>
        Task<IEnumerable<Baggage>> GetByReservationCodeAsync(string reservationCode);

        /// <summary>
        /// Gets a baggage record by its identifier, or returns null when no match exists.
        /// </summary>
        Task<Baggage?> GetByIdAsync(int baggageId);

        /// <summary>
        /// Counts the baggage records linked to a reservation code.
        /// </summary>
        Task<int> CountByReservationCodeAsync(string reservationCode);
    }
}
