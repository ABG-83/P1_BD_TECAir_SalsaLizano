// =============================================================================
// File    : IAirportRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for airport records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for airport records.
    /// </summary>
    public interface IAirportRepository
    {
        /// <summary>
        /// Gets all airports registered in the system.
        /// </summary>
        Task<IEnumerable<Airport>> GetAllAsync();

        /// <summary>
        /// Gets an airport by its unique identifier, or returns null when no match exists.
        /// </summary>
        Task<Airport?> GetByIdAsync(int airportId);
    }
}
