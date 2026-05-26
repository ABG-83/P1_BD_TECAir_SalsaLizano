// =============================================================================
// File    : BaggageRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements baggage persistence logic with ADO.NET.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IBaggageRepository"/> using native ADO.NET.
    /// </summary>
    public class BaggageRepository(IDbConnectionFactory connectionFactory) : IBaggageRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Maps a data reader row into a <see cref="Baggage"/> domain object.
        /// </summary>
        private static Baggage MapRow(IDataReader r) => new()
        {
            BaggageId = r.GetInt32(r.GetOrdinal("baggage_id")),
            Weight = r.GetDecimal(r.GetOrdinal("weight")),
            Color = r.GetString(r.GetOrdinal("color")),
            ReservationCode = r.GetString(r.GetOrdinal("reservation_code")),
            UserId = r.GetInt32(r.GetOrdinal("user_id"))
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
        public async Task<int> CreateAsync(Baggage baggage)
        {
            const string sql = """
                INSERT INTO baggages (weight, color, reservation_code, user_id)
                VALUES (@weight, @color, @reservationCode, @userId)
                RETURNING baggage_id;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "weight", baggage.Weight);
            AddParam(command, "color", baggage.Color);
            AddParam(command, "reservationCode", baggage.ReservationCode);
            AddParam(command, "userId", baggage.UserId);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Baggage>> GetByReservationCodeAsync(string reservationCode)
        {
            const string sql = """
                SELECT baggage_id, weight, color, reservation_code, user_id
                FROM baggages
                WHERE reservation_code = @reservationCode;
                """;

            var baggages = new List<Baggage>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationCode", reservationCode);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                baggages.Add(MapRow(reader));

            return baggages;
        }

        /// <inheritdoc />
        public async Task<Baggage?> GetByIdAsync(int baggageId)
        {
            const string sql = """
                SELECT baggage_id, weight, color, reservation_code, user_id
                FROM baggages
                WHERE baggage_id = @baggageId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "baggageId", baggageId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        /// <inheritdoc />
        public async Task<int> CountByReservationCodeAsync(string reservationCode)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM baggages
                WHERE reservation_code = @reservationCode;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationCode", reservationCode);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
    }
}
