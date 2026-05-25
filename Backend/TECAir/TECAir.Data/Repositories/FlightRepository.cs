// =============================================================================
// Archivo  : FlightRepository.cs
// Capa     : TECAir.Data → Repositories
// Propósito: Implementación de IFlightRepository usando ADO.NET puro.
//            Maneja las tablas VUELO (flights) y VUELO_ESCALA (flight_routes).
//
//            Flujo de registro de un vuelo nuevo:
//              1. CreateAsync()   → INSERT en flights
//              2. AddStopAsync()  → INSERT en flight_routes (una vez por escala)
// =============================================================================


using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IFlightRepository"/> using native ADO.NET.
    /// </summary>
    public class FlightRepository(IDbConnectionFactory connectionFactory) : IFlightRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a single row from the data reader into a <see cref="Flight"/> domain object.
        /// </summary>
        private static Flight MapRow(IDataReader r) => new()
        {
            FlightNumber = r.GetString(r.GetOrdinal("flight_number")),
            DepartureTime = r.GetDateTime(r.GetOrdinal("departure_time")),
            ArrivalTime = r.GetDateTime(r.GetOrdinal("arrival_time")),
            Status = Enum.Parse<FlightStatus>(r.GetString(r.GetOrdinal("status"))),
            AirplanePlateNumber = r.GetString(r.GetOrdinal("airplane_plate_number")),
            OriginAirportId = r.GetInt32(r.GetOrdinal("origin_airport_id")),
            DestinationAirportId = r.GetInt32(r.GetOrdinal("destination_airport_id"))
        };


        // Mapea una fila del DataReader a un objeto FlightRoute (escala)
        private static FlightRoute MapStopRow(IDataReader r) => new()
        {
            FlightNumber = r.GetString(r.GetOrdinal("flight_number")),
            AirportId = r.GetInt32(r.GetOrdinal("airport_id")),
            StopOrder = r.GetInt32(r.GetOrdinal("stop_order"))
        };

        // Crea y agrega un parámetro al comando
        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        // ── Queries ────────────────────────────────────────────────────────────

        public async Task<IEnumerable<Flight>> GetAllAsync()
        {
            const string sql = """
                SELECT flight_number, departure_time, arrival_time, status,
                       airplane_plate_number, origin_airport_id, destination_airport_id
                FROM flights
                ORDER BY departure_time ASC;
                """;

            var flights = new List<Flight>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
                flights.Add(MapRow(reader));

            return flights;
        }

        public async Task<Flight?> GetByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT flight_number, departure_time, arrival_time, status,
                       airplane_plate_number, origin_airport_id, destination_airport_id
                FROM flights
                WHERE flight_number = @flightNumber;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        public async Task<IEnumerable<FlightRoute>> GetStopsByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT flight_number, airport_id, stop_order
                FROM flight_routes
                WHERE flight_number = @flightNumber
                ORDER BY stop_order ASC;
                """;

            var stops = new List<FlightRoute>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                stops.Add(MapStopRow(reader));

            return stops;
        }

        public async Task CreateAsync(Flight flight)
        {
            const string sql = """
                INSERT INTO flights (
                    flight_number, departure_time, arrival_time, status,
                    airplane_plate_number, origin_airport_id, destination_airport_id
                )
                VALUES (
                    @flightNumber, @departureTime, @arrivalTime, @status,
                    @airplanePlateNumber, @originAirportId, @destinationAirportId
                );
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "flightNumber", flight.FlightNumber);
            AddParam(command, "departureTime", flight.DepartureTime);
            AddParam(command, "arrivalTime", flight.ArrivalTime);
            AddParam(command, "status", flight.Status.ToString());
            AddParam(command, "airplanePlateNumber", flight.AirplanePlateNumber);
            AddParam(command, "originAirportId", flight.OriginAirportId);
            AddParam(command, "destinationAirportId", flight.DestinationAirportId);

            command.ExecuteNonQuery();
        }

        public async Task AddStopAsync(FlightRoute stop)
        {
            const string sql = """
                INSERT INTO flight_routes (flight_number, airport_id, stop_order)
                VALUES (@flightNumber, @airportId, @stopOrder);
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "flightNumber", stop.FlightNumber);
            AddParam(command, "airportId", stop.AirportId);
            AddParam(command, "stopOrder", stop.StopOrder);

            command.ExecuteNonQuery();
        }

        // Actualiza solo la columna 'status' del vuelo identificado por su número
        public async Task UpdateStatusAsync(string flightNumber, FlightStatus status)
        {
            const string sql = """
                UPDATE flights
                SET status = @status
                WHERE flight_number = @flightNumber;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "status", status.ToString());
            AddParam(command, "flightNumber", flightNumber);

            command.ExecuteNonQuery();
        }
        // Búsqueda por ruta origen → destino. Retorna vuelos que tengan esa ruta, incluyendo los que tengan escalas intermedias. 
        /// <inheritdoc />
        public async Task<IEnumerable<Flight>> GetFlightsByRouteAsync(int originId, int destinationId)
        {
            const string query = """
                SELECT flight_number, departure_time, arrival_time, status, 
                       airplane_plate_number, origin_airport_id, destination_airport_id
                FROM flights
                WHERE origin_airport_id = @originId 
                  AND destination_airport_id = @destinationId
                ORDER BY departure_time ASC;
                """;

            var flights = new List<Flight>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            // Define Origin Parameter
            var pOrigin = command.CreateParameter();
            pOrigin.ParameterName = "@originId";
            pOrigin.Value = originId;
            command.Parameters.Add(pOrigin);

            // Define Destination Parameter
            var pDestination = command.CreateParameter();
            pDestination.ParameterName = "@destinationId";
            pDestination.Value = destinationId;
            command.Parameters.Add(pDestination);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                flights.Add(MapRow(reader));
            }

            return flights;
        }

        /// <inheritdoc />
        public async Task<int> GetCapacityByFlightNumberAsync(string flightNumber)
        {
            // Join the flight directory tracking table with the physical airplane metadata table
            // to extract the absolute maximum seating capacity allocation limit.
            const string sql = @"
                SELECT a.seat_count
                FROM flights f
                INNER JOIN airplanes a ON f.airplane_plate_number = a.plate_number
                WHERE f.flight_number = @FlightNumber;";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            // Manual abstract parameter setup binding
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@FlightNumber";
            parameter.Value = flightNumber ?? (object)DBNull.Value;
            command.Parameters.Add(parameter);

            // Execute scalar since we expect an absolute integer value back from the engine block
            var result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                // Return 0 as an indicator flag to notify the service that the flight manifest is uninitialized
                return 0;
            }

            return Convert.ToInt32(result);
        }
    }
}
