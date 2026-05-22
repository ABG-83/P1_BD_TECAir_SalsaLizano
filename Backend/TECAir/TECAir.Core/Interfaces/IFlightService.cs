using TECAir.Data.Models;

namespace TECAir.Core.Interfaces
{
    /// <summary>
    /// Defines business logic operations for processing and filtering flights.
    /// </summary>
    public interface IFlightService
    {
        /// <summary>
        /// Searches and filters flights executing a specific route from origin to destination.
        /// </summary>
        /// <param name="originAirportId">The primary key identifier of the origin airport.</param>
        /// <param name="destinationAirportId">The primary key identifier of the destination airport.</param>
        /// <returns>A collection of matching available flights sorted chronologically.</returns>
        /// <exception cref="ArgumentException">Thrown when input IDs violate business safety guards.</exception>
        Task<IEnumerable<Flight>> SearchFlightsByRouteAsync(int originAirportId, int destinationAirportId);
    }
}
