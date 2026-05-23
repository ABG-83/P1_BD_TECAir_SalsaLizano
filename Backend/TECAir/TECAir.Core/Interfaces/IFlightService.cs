// =============================================================================
// Archivo  : IFlightService.cs
// Capa     : TECAir.Core → Interfaces
// Propósito: Contrato de la lógica de negocio para vuelos.
//
//            Scope del Issue #14 (gestión de vuelos):
//              - Registrar un vuelo con ruta completa (origen, escalas, destino)
//              - Consultar todos los vuelos
//              - Consultar un vuelo por número
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
        
        // Retorna todos los vuelos con su ruta enriquecida (nombres de aeropuertos + escalas)
        Task<IEnumerable<FlightResponseDto>> GetAllFlightsAsync();
 
        // Retorna un vuelo específico. Retorna null si no existe.
        Task<FlightResponseDto?> GetFlightByNumberAsync(string flightNumber);
 
        // Registra un vuelo nuevo con su ruta completa.
        // Lanza InvalidOperationException si el avión o algún aeropuerto no existe.
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
