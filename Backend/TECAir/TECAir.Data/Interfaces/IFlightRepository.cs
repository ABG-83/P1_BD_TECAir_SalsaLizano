// =============================================================================
// Archivo  : IFlightRepository.cs
// Capa     : TECAir.Data → Interfaces
// Propósito: Contrato de acceso a datos para las tablas VUELO y VUELO_ESCALA.
//            Se agrupan en un solo repositorio porque registrar un vuelo
//            implica siempre insertar también sus escalas.
//
//            Métodos del Issue #14 (gestión de vuelos):
//              - GetAllAsync          → listar todos los vuelos
//              - GetByFlightNumberAsync → obtener un vuelo por número
//              - GetStopsByFlightNumberAsync → escalas de un vuelo
//              - CreateAsync          → registrar un vuelo nuevo
//              - AddStopAsync         → agregar una escala al vuelo recién creado
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing <see cref="Flight"/> records.
    /// </summary>
    public interface IFlightRepository
    {
        // Retorna todos los vuelos ordenados por hora de salida
        Task<IEnumerable<Flight>> GetAllAsync();
 
        // Busca un vuelo por su número de vuelo. Retorna null si no existe.
        Task<Flight?> GetByFlightNumberAsync(string flightNumber);
 
        // Retorna las escalas de un vuelo ordenadas por StopOrder (1, 2, 3...)
        Task<IEnumerable<FlightRoute>> GetStopsByFlightNumberAsync(string flightNumber);
 
        // Inserta un nuevo vuelo en la tabla VUELO
        Task CreateAsync(Flight flight);
 
        // Inserta una escala en VUELO_ESCALA. Se llama una vez por cada aeropuerto intermedio.
        Task AddStopAsync(FlightRoute stop);
        // Busqueda por ruta origen → destino. Retorna vuelos que tengan esa ruta, incluyendo los que tengan escalas intermedias.
        Task<IEnumerable<Flight>> GetFlightsByRouteAsync(int originId, int destinationId);
    }
}
