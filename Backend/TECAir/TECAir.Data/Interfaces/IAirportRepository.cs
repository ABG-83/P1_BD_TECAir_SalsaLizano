using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing <see cref="Airport"/> records.
    /// </summary>
    public interface IAirportRepository
    {
        /// <summary>
        /// Retrieves all airports registered in the system to populate origin and destination lists.
        /// </summary>
        /// <returns>A collection of all available airports.</returns>
        Task<IEnumerable<Airport>> GetAllAsync();

        /// <summary>
        /// Finds a specific airport by its unique identifier code.
        /// </summary>
        /// <param name="airportId">The unique IATA code of the airport.</param>
        /// <returns>The airport profile if found; otherwise, null.</returns>
        Task<Airport?> GetByIdAsync(int airportId);
    }
}
