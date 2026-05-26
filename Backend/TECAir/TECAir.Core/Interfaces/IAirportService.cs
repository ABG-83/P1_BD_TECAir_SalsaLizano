// =============================================================================
// File    : IAirportService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for airport operations.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Core.Interfaces
{
    /// <summary>
    /// Defines business logic operations for handling airport information.
    /// </summary>
    public interface IAirportService
    {
        /// <summary>
        /// Retrieves a list of all available airports for flight selection.
        /// </summary>
        /// <returns>A collection of <see cref="Airport"/> objects.</returns>
        Task<IEnumerable<Airport>> GetAllAirportsAsync();

        /// <summary>
        /// Retrieves detailed information for a specific airport.
        /// </summary>
        /// <param name="id">The unique numerical identifier of the airport.</param>
        /// <returns>The airport details or null if not found.</returns>
        Task<Airport?> GetAirportByIdAsync(int id);
    }
}
