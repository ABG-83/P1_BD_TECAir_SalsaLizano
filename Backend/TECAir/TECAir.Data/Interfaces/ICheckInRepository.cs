// =============================================================================
// File    : ICheckInRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for check-in records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for check-in records.
    /// </summary>
    public interface ICheckInRepository
    {
        /// <summary>
        /// Creates a check-in record and returns the generated identifier.
        /// </summary>
        Task<int> CreateAsync(CheckIn checkIn);

        /// <summary>
        /// Gets a check-in by its identifier, or returns null when no match exists.
        /// </summary>
        Task<CheckIn?> GetByIdAsync(int checkInId);

        /// <summary>
        /// Gets the check-in associated with a reservation code, or returns null when none exists.
        /// </summary>
        Task<CheckIn?> GetByReservationCodeAsync(string reservationCode);

        /// <summary>
        /// Gets all check-ins registered for a flight.
        /// </summary>
        Task<IEnumerable<CheckIn>> GetByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Determines whether a seat is already occupied on a flight.
        /// </summary>
        Task<bool> IsSeatTakenAsync(string flightNumber, string seat);
    }
}
