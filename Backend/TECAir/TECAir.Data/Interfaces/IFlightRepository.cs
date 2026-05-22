using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing <see cref="Flight"/> records.
    /// </summary>
    public interface IFlightRepository
    {
        /// <summary>
        /// Retrieves all flights matching a specific origin and destination airport identifier.
        /// </summary>
        /// <param name="originId">The identifier of the origin airport.</param>
        /// <param name="destinationId">The identifier of the destination airport.</param>
        /// <returns>A collection of matching flights.</returns>
        Task<IEnumerable<Flight>> GetFlightsByRouteAsync(int originId, int destinationId);
    }
}
