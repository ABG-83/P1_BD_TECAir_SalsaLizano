// =============================================================================
// File    : IFlightService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for flight operations.
// =============================================================================

using TECAir.Core.DTOs.Flights;
using TECAir.Data.Models;

namespace TECAir.Core.Interfaces
{
    /// <summary>
    /// Defines business logic operations for processing and filtering flights.
    /// </summary>
    public interface IFlightService
    {
        
        // Returns all flights with their enriched route (airport names + stopovers).
        Task<IEnumerable<FlightResponseDto>> GetAllFlightsAsync();
 
        // Returns a specific flight. Returns null when it does not exist.
        Task<FlightResponseDto?> GetFlightByNumberAsync(string flightNumber);
 
        // Registers a new flight with its full route.
        // Throws InvalidOperationException when the airplane or an airport does not exist.
        Task CreateFlightAsync(CreateFlightDto dto);

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
