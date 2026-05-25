using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IReservationRepository"/> using native ADO.NET.
    /// </summary>
    public class ReservationRepository(IDbConnectionFactory connectionFactory) : IReservationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a single row from the data reader into a <see cref="Reservation"/> domain object.
        /// </summary>
        private static Reservation MapRow(IDataReader r) => new()
        {
            ReservationCode = r.GetString(r.GetOrdinal("reservation_code")),
            Date = r.GetDateTime(r.GetOrdinal("date")),
            PaymentState = Enum.Parse<PaymentStatus>(r.GetString(r.GetOrdinal("payment_state"))),
            UserId = r.GetInt32(r.GetOrdinal("user_id")),
            FlightNumber = r.GetString(r.GetOrdinal("flight_number"))
        };

        /// <summary>
        /// Utility helper to abstract and chain parameter creation cleanly.
        /// </summary>
        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        // ── Queries ───────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<string> CreateAsync(Reservation reservation)
        {
            const string query = """
                INSERT INTO reservations (reservation_code, date, payment_state, user_id, flight_number)
                VALUES (@code, @date, @state, @userId, @flightNum)
                RETURNING reservation_code;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            AddParameter(command, "@code", reservation.ReservationCode);
            AddParameter(command, "@date", reservation.Date);
            AddParameter(command, "@state", reservation.PaymentState.ToString());
            AddParameter(command, "@userId", reservation.UserId);
            AddParameter(command, "@flightNum", reservation.FlightNumber);

            var result = await Task.Run(() => command.ExecuteScalar());
            return result?.ToString() ?? string.Empty;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Reservation reservation)
        {
            const string query = """
                UPDATE reservations 
                SET payment_state = @state, 
                    date = @date
                WHERE reservation_code = @code;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            AddParameter(command, "@state", reservation.PaymentState.ToString());
            AddParameter(command, "@date", reservation.Date);
            AddParameter(command, "@code", reservation.ReservationCode);

            int rowsAffected = await Task.Run(() => command.ExecuteNonQuery());
            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<bool> CancelAsync(string reservationCode)
        {
            // Siguiendo buenas prácticas de negocio aéreo, cancelamos cambiando el estado. 
            // Si tu base prefiere un DELETE físico, cambialo a: DELETE FROM reservations WHERE...
            const string query = """
                UPDATE reservations 
                SET payment_state = 'Failed' 
                WHERE reservation_code = @code;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            AddParameter(command, "@code", reservationCode);

            int rowsAffected = await Task.Run(() => command.ExecuteNonQuery());
            return rowsAffected > 0;
        }

        // ── Queries ────────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<Reservation?> GetByCodeAsync(string reservationCode)
        {
            const string query = """
                SELECT reservation_code, date, payment_state, user_id, flight_number 
                FROM reservations 
                WHERE reservation_code = @code;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            AddParameter(command, "@code", reservationCode);

            using var reader = await Task.Run(() => command.ExecuteReader());
            return reader.Read() ? MapRow(reader) : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            const string query = """
                SELECT reservation_code, date, payment_state, user_id, flight_number 
                FROM reservations 
                WHERE user_id = @userId 
                ORDER BY date DESC;
                """;

            var reservations = new List<Reservation>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            AddParameter(command, "@userId", userId);

            using var reader = await Task.Run(() => command.ExecuteReader());
            while (reader.Read())
            {
                reservations.Add(MapRow(reader));
            }

            return reservations;
        }

        // Retorna solo las reservaciones pagadas de un vuelo, que son los pasajeros confirmados
        public async Task<IEnumerable<Reservation>> GetPaidByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT reservation_id, date, payment_status, user_id, flight_number
                FROM reservations
                WHERE flight_number = @flightNumber
                  AND payment_status = 'paid'
                ORDER BY date ASC;
            """;

            var reservations = new List<Reservation>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "flightNumber", flightNumber);

            return reservations;
        }

        /// <inheritdoc />
        public async Task<int> GetActiveCountByFlightNumberAsync(string flightNumber)
        {
            // Count rows tracking active inventory spaces, filtering out refunded blocks 
            // so those seats become instantly vacant and available for sale again.
            const string sql = @"
                SELECT COUNT(*) 
                FROM reservations 
                WHERE flight_number = @FlightNumber 
                  AND payment_state <> @RefundedStatus;";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            // Parameterized bindings matching the real enum mapping rules
            var flightParam = command.CreateParameter();
            flightParam.ParameterName = "@FlightNumber";
            flightParam.Value = flightNumber ?? (object)DBNull.Value;
            command.Parameters.Add(flightParam);

            var statusParam = command.CreateParameter();
            statusParam.ParameterName = "@RefundedStatus";
            statusParam.Value = PaymentStatus.Refunded.ToString(); // Or integer if stored as int in DB
            command.Parameters.Add(statusParam);

            var result = command.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
