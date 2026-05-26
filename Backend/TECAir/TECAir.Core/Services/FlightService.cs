// =============================================================================
// File    : FlightService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for flight operations.
// =============================================================================

using TECAir.Core.DTOs.Flights;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{

    /// Coordinates business rule execution and workflows associated with flights.
    public class FlightService(
        IFlightRepository flightRepository,
        IAirplaneRepository airplaneRepository,
        IAirportRepository airportRepository) : IFlightService
    {
        private readonly IFlightRepository _flightRepository = flightRepository;
        private readonly IAirplaneRepository _airplaneRepository = airplaneRepository;
        private readonly IAirportRepository  _airportRepository  = airportRepository;

           // ── Consultas ──────────────────────────────────────────────────────────
 
        public async Task<IEnumerable<FlightResponseDto>> GetAllFlightsAsync()
        {
            var flights = await _flightRepository.GetAllAsync();
 
            var result = new List<FlightResponseDto>();
            foreach (var flight in flights)
                result.Add(await BuildResponseDtoAsync(flight));
 
            return result;
        }
 
        public async Task<FlightResponseDto?> GetFlightByNumberAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber);
 
            // El controlador convierte null en 404 Not Found
            if (flight is null) return null;
 
            return await BuildResponseDtoAsync(flight);
        }
 
        // ── Registro de vuelo ────────────────────
 
        public async Task CreateFlightAsync(CreateFlightDto dto)
        {
            // Validate that the flight number is not already registered.
            var existing = await _flightRepository.GetByFlightNumberAsync(dto.FlightNumber);
            if (existing is not null)
                throw new InvalidOperationException($"Ya existe un vuelo con el número '{dto.FlightNumber}'.");
 
            // Validate that the airplane exists.
            var airplane = await _airplaneRepository.GetByPlateNumberAsync(dto.AirplanePlateNumber)
                ?? throw new InvalidOperationException($"No existe ningún avión con matrícula '{dto.AirplanePlateNumber}'.");
 
            // Validate that the origin and destination exist in the database.
            var origin = await _airportRepository.GetByIdAsync(dto.OriginAirportId)
                ?? throw new InvalidOperationException($"No existe el aeropuerto de origen con ID {dto.OriginAirportId}.");
 
            var destination = await _airportRepository.GetByIdAsync(dto.DestinationAirportId)
                ?? throw new InvalidOperationException($"No existe el aeropuerto de destino con ID {dto.DestinationAirportId}.");
 
            // Validate business rules.
            if (dto.OriginAirportId == dto.DestinationAirportId)
                throw new InvalidOperationException("El aeropuerto de origen y destino no pueden ser el mismo.");
 
            if (dto.ArrivalTime <= dto.DepartureTime)
                throw new InvalidOperationException("La hora de llegada debe ser posterior a la hora de salida.");
 
            // Validate that all stopover airports exist before inserting anything.
            foreach (var stopId in dto.StopAirportIds)
            {
                _ = await _airportRepository.GetByIdAsync(stopId)
                    ?? throw new InvalidOperationException($"No existe el aeropuerto de escala con ID {stopId}.");
            }
 
            // Insert the flight into the database.
            var flight = new Flight
            {
                FlightNumber         = dto.FlightNumber,
                DepartureTime        = dto.DepartureTime,
                ArrivalTime          = dto.ArrivalTime,
                Status               = FlightStatus.Scheduled,  // Todo vuelo nuevo inicia en Scheduled
                AirplanePlateNumber  = dto.AirplanePlateNumber,
                OriginAirportId      = dto.OriginAirportId,
                DestinationAirportId = dto.DestinationAirportId
            };
 
            await _flightRepository.CreateAsync(flight);
 
            // Insert stops in order — the index + 1 defines the StopOrder (1, 2, 3...).
            for (int i = 0; i < dto.StopAirportIds.Count; i++)
            {
                await _flightRepository.AddStopAsync(new FlightRoute
                {
                    FlightNumber = dto.FlightNumber,
                    AirportId    = dto.StopAirportIds[i],
                    StopOrder    = i + 1
                });
            }
        }

        
        // Busqueda 
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
    
        /// <inheritdoc />
        public async Task UpdateFlightStatusAsync(string flightNumber, string status)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            var mapped = status.ToLower() switch
            {
                "abierto"    => FlightStatus.Boarding,
                "cerrado"    => FlightStatus.Landed,
                "cancelado"  => FlightStatus.Cancelled,
                "delayed"    => FlightStatus.Delayed,
                "inair"      => FlightStatus.InAir,
                "boarding"   => FlightStatus.Boarding,
                "scheduled"  => FlightStatus.Scheduled,
                "landed"     => FlightStatus.Landed,
                "cancelled"  => FlightStatus.Cancelled,
                _            => FlightStatus.Scheduled,
            };

            await _flightRepository.UpdateStatusAsync(flightNumber, mapped);
        }

        // ── Private helper method ────────────────────────────────────────────
 
        // Construye un FlightResponseDto enriquecido a partir del modelo plano Flight.
        // Consulta la BD para resolver los nombres de todos los aeropuertos de la ruta.
        private async Task<FlightResponseDto> BuildResponseDtoAsync(Flight flight)
        {
            var airplane    = await _airplaneRepository.GetByPlateNumberAsync(flight.AirplanePlateNumber);
            var origin      = await _airportRepository.GetByIdAsync(flight.OriginAirportId);
            var destination = await _airportRepository.GetByIdAsync(flight.DestinationAirportId);
            var stops       = await _flightRepository.GetStopsByFlightNumberAsync(flight.FlightNumber);
 
            // Para cada escala, resolvemos el nombre del aeropuerto
            var stopDtos = new List<FlightStopDto>();
            foreach (var stop in stops)
            {
                var stopAirport = await _airportRepository.GetByIdAsync(stop.AirportId);
                stopDtos.Add(new FlightStopDto
                {
                    Order     = stop.StopOrder,
                    AirportId = stop.AirportId,
                    Name      = stopAirport?.Name     ?? "Desconocido",
                    Location  = stopAirport?.Location ?? "Desconocida"
                });
            }
 
            return new FlightResponseDto
            {
                FlightNumber        = flight.FlightNumber,
                DepartureTime       = flight.DepartureTime,
                ArrivalTime         = flight.ArrivalTime,
                Status              = flight.Status.ToString(),
                AirplanePlateNumber = flight.AirplanePlateNumber,
                PassengerCapacity   = airplane?.PassengerCapacity ?? 0,
                Origin = new AirportSummaryDto
                {
                    AirportId = origin?.AirportId ?? 0,
                    Name      = origin?.Name      ?? "Desconocido",
                    Location  = origin?.Location  ?? "Desconocida"
                },
                Destination = new AirportSummaryDto
                {
                    AirportId = destination?.AirportId ?? 0,
                    Name      = destination?.Name      ?? "Desconocido",
                    Location  = destination?.Location  ?? "Desconocida"
                },
                Stops = stopDtos
            };
        }
    }
}
