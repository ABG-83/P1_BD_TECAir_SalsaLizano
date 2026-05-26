// =============================================================================
// File    : AirplaneRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements airplane persistence logic with ADO.NET.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IAirplaneRepository"/> using native ADO.NET.
    /// </summary>
    public class AirplaneRepository(IDbConnectionFactory connectionFactory) : IAirplaneRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Maps a data reader row into an <see cref="Airplane"/> domain object.
        /// </summary>
        private static Airplane MapRow(IDataReader r) => new()
        {
            PlateNumber = r.GetString(r.GetOrdinal("plate_number")),
            PassengerCapacity = r.GetInt32(r.GetOrdinal("passenger_capacity")),
            SeatCount = r.GetInt32(r.GetOrdinal("seat_count"))
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
        public async Task<IEnumerable<Airplane>> GetAllAsync()
        {
            const string sql = """
                SELECT plate_number, passenger_capacity, seat_count
                FROM airplanes
                ORDER BY plate_number ASC;
                """;

            var airplanes = new List<Airplane>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
                airplanes.Add(MapRow(reader));

            return airplanes;
        }

        /// <inheritdoc />
        public async Task<Airplane?> GetByPlateNumberAsync(string plateNumber)
        {
            const string sql = """
                SELECT plate_number, passenger_capacity, seat_count
                FROM airplanes
                WHERE plate_number = @plateNumber;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "plateNumber", plateNumber);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }
    }
}
