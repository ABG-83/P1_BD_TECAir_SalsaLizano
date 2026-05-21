using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IAirportRepository"/> using native ADO.NET.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AirportRepository"/> class.
    /// </remarks>
    /// <param name="connectionFactory">The factory responsible for generating database connections.</param>
    public class AirportRepository(IDbConnectionFactory connectionFactory) : IAirportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a single row from the data reader into an <see cref="Airport"/> domain object.
        /// </summary>
        private static Airport MapRow(IDataReader r) => new()
        {
            AirportId = r.GetInt32(r.GetOrdinal("airport_id")),
            Name = r.GetString(r.GetOrdinal("name")),
            Location = r.GetString(r.GetOrdinal("location"))
        };

        // ── Queries ────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<IEnumerable<Airport>> GetAllAsync()
        {
            const string query = """
                SELECT airport_id, name, location 
                FROM airports 
                ORDER BY name ASC;
                """;

            var airports = new List<Airport>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                airports.Add(MapRow(reader));
            }

            return airports;
        }

        /// <inheritdoc />
        public async Task<Airport?> GetByIdAsync(int airportId)
        {
            const string query = """
                SELECT airport_id, name, location 
                FROM airports 
                WHERE airport_id = @id;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = airportId;
            command.Parameters.Add(parameter);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRow(reader);
            }

            return null;
        }
    }
}
