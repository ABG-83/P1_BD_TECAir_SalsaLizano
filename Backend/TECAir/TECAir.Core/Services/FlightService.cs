using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    /// <summary>
    /// Coordinates business rule execution and workflows associated with flights.
    /// </summary>
    public class FlightService(IFlightRepository flightRepository) : IFlightService
    {
        private readonly IFlightRepository _flightRepository = flightRepository;

        /// <inheritdoc />
        public async Task<IEnumerable<Flight>> SearchFlightsByRouteAsync(int originAirportId, int destinationAirportId)
        {
            // ── Business Rule Validation ──────────────────────────────────────
            if (originAirportId <= 0 || destinationAirportId <= 0)
            {
                throw new ArgumentException("Airport identifiers must be valid, positive integers.");
            }

            if (originAirportId == destinationAirportId)
            {
                throw new ArgumentException("Origin and destination airports cannot be identical.");
            }

            // ── Data Acquisition ──────────────────────────────────────────────
            return await _flightRepository.GetFlightsByRouteAsync(originAirportId, destinationAirportId);
        }
    }
}
