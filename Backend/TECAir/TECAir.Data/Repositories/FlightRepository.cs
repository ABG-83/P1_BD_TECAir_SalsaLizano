// =============================================================================
// File    : FlightRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements flight and stop persistence logic with ADO.NET.
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

        /// <summary>
        /// Maps a flight row into a <see cref="Flight"/> domain object.
        /// </summary>
        private static Flight MapRow(IDataReader r) => new()
        {
            FlightNumber = r.GetString(r.GetOrdinal("flight_number")),
            DepartureTime = r.GetDateTime(r.GetOrdinal("departure_time")),
            ArrivalTime = r.GetDateTime(r.GetOrdinal("arrival_time")),
            Status = Enum.Parse<FlightStatus>(r.GetString(r.GetOrdinal("status"))),
            AirplanePlateNumber = r.GetString(r.GetOrdinal("airplane_plate_number")),
            OriginAirportId = r.GetInt32(r.GetOrdinal("origin_airport_id")),
            DestinationAirportId = r.GetInt32(r.GetOrdinal("destination_airport_id")),
            OriginAirportName = r.GetString(r.GetOrdinal("origin_airport_name")),
            DestinationAirportName = r.GetString(r.GetOrdinal("destination_airport_name"))
        };

        /// <summary>
        /// Maps a route stop row into a <see cref="FlightRoute"/> domain object.
        /// </summary>
        private static FlightRoute MapStopRow(IDataReader r) => new()
        {
            FlightNumber = r.GetString(r.GetOrdinal("flight_number")),
            AirportId = r.GetInt32(r.GetOrdinal("airport_id")),
            StopOrder = r.GetInt32(r.GetOrdinal("stop_order"))
        };

        /// <summary>
        /// Creates and adds a command parameter.
        /// </summary>
        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Flight>> GetAllAsync()
        {
            const string sql = """
                    SELECT f.flight_number, f.departure_time, f.arrival_time, f.status,
                    f.airplane_plate_number, f.origin_airport_id, f.destination_airport_id,
                    a1.name AS origin_airport_name, 
                    a2.name AS destination_airport_name
                FROM flights f
                LEFT JOIN airports a1 ON f.origin_airport_id = a1.airport_id
                LEFT JOIN airports a2 ON f.destination_airport_id = a2.airport_id
                ORDER BY f.departure_time ASC;
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

        /// <inheritdoc />
        public async Task<Flight?> GetByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT f.flight_number, f.departure_time, f.arrival_time, f.status,
                       f.airplane_plate_number, f.origin_airport_id, f.destination_airport_id,
                       COALESCE(a1.name, '') AS origin_airport_name,
                       COALESCE(a2.name, '') AS destination_airport_name
                FROM flights f
                LEFT JOIN airports a1 ON f.origin_airport_id = a1.airport_id
                LEFT JOIN airports a2 ON f.destination_airport_id = a2.airport_id
                WHERE f.flight_number = @flightNumber;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public async Task<IEnumerable<Flight>> GetFlightsByRouteAsync(int originId, int destinationId)
        {
            const string query = """
                SELECT f.flight_number, f.departure_time, f.arrival_time, f.status, 
                    f.airplane_plate_number, f.origin_airport_id, f.destination_airport_id,
                    a1.name AS origin_airport_name, 
                    a2.name AS destination_airport_name
                FROM flights f
                LEFT JOIN airports a1 ON f.origin_airport_id = a1.airport_id
                LEFT JOIN airports a2 ON f.destination_airport_id = a2.airport_id
                WHERE f.origin_airport_id = @originId 
                AND f.destination_airport_id = @destinationId
                ORDER BY f.departure_time ASC;
                """;

            var flights = new List<Flight>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            var pOrigin = command.CreateParameter();
            pOrigin.ParameterName = "@originId";
            pOrigin.Value = originId;
            command.Parameters.Add(pOrigin);

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
            const string sql = @"
                SELECT a.seat_count
                FROM flights f
                INNER JOIN airplanes a ON f.airplane_plate_number = a.plate_number
                WHERE f.flight_number = @FlightNumber;";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@FlightNumber";
            parameter.Value = flightNumber ?? (object)DBNull.Value;
            command.Parameters.Add(parameter);

            var result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(result);
        }
    }
}
