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

        // ── Queries ────────────────────────────────────────────────────────────

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
    }
}
