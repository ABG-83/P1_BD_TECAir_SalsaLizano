using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    /// <summary>
    /// Implementation of <see cref="IAirportService"/> that orchestrates data access 
    /// through the airport repository.
    /// </summary>
    public class AirportService : IAirportService
    {
        private readonly IAirportRepository _airportRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AirportService"/> class.
        /// </summary>
        /// <param name="airportRepository">The repository used for database operations.</param>
        public AirportService(IAirportRepository airportRepository)
        {
            _airportRepository = airportRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Airport>> GetAllAirportsAsync()
        {
            return await _airportRepository.GetAllAsync();
        }

        /// <inheritdoc />
        public async Task<Airport?> GetAirportByIdAsync(int id)
        {
            if (id <= 0) return null;

            return await _airportRepository.GetByIdAsync(id);
        }
    }
}
