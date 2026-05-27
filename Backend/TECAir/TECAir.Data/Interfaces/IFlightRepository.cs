// =============================================================================
// File    : IFlightRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for flight and flight-route records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for flight and flight-route records.
    /// </summary>
    public interface IFlightRepository
    {
        /// <summary>
        /// Gets all flights ordered by departure time.
        /// </summary>
        Task<IEnumerable<Flight>> GetAllAsync();

        /// <summary>
        /// Gets a flight by its flight number, or returns null when no match exists.
        /// </summary>
        Task<Flight?> GetByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Gets the stops registered for a flight ordered by stop order.
        /// </summary>
        Task<IEnumerable<FlightRoute>> GetStopsByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Creates a new flight record.
        /// </summary>
        Task CreateAsync(Flight flight);

        /// <summary>
        /// Adds a stop to a flight route.
        /// </summary>
        Task AddStopAsync(FlightRoute stop);

        /// <summary>
        /// Gets flights that match the requested origin and destination airports.
        /// </summary>
        Task<IEnumerable<Flight>> GetFlightsByRouteAsync(int originId, int destinationId);

        /// <summary>
        /// Updates the operational status for a flight.
        /// </summary>
        Task UpdateStatusAsync(string flightNumber, FlightStatus newStatus);

        /// <summary>
        /// Gets the seat capacity configured for the airplane assigned to a flight.
        /// </summary>
        Task<int> GetCapacityByFlightNumberAsync(string flightNumber);

        /// <summary>
        /// Deletes a flight and its associated routes. Returns false when not found.
        /// </summary>
        Task<bool> DeleteAsync(string flightNumber);

        /// <summary>
        /// Updates the mutable fields of a flight. Returns false when not found.
        /// </summary>
        Task<bool> UpdateAsync(Flight flight);
    }
}
