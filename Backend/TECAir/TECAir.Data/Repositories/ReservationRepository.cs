// =============================================================================
// File    : ReservationRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements reservation persistence logic with ADO.NET.
// =============================================================================

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

        /// <summary>
        /// Maps a reservation row into a <see cref="Reservation"/> domain object.
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
        /// Maps a reservation row that includes a joined user_name column.
        /// </summary>
        private static Reservation MapRowFull(IDataReader r)
        {
            var res = MapRow(r);
            var ord = r.GetOrdinal("user_name");
            res.UserName = r.IsDBNull(ord) ? null : r.GetString(ord);
            return res;
        }

        /// <summary>
        /// Creates and adds a command parameter.
        /// </summary>
        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            const string query = """
                SELECT r.reservation_code, r.date, r.payment_state, r.user_id, r.flight_number,
                       COALESCE(u.full_name, '') AS user_name
                FROM reservations r
                LEFT JOIN users u ON r.user_id = u.user_id
                ORDER BY r.date DESC;
                """;

            var reservations = new List<Reservation>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await Task.Run(() => command.ExecuteReader());
            while (reader.Read())
                reservations.Add(MapRowFull(reader));

            return reservations;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Reservation>> SearchByNameAsync(string name)
        {
            const string query = """
                SELECT r.reservation_code, r.date, r.payment_state, r.user_id, r.flight_number,
                       COALESCE(u.full_name, '') AS user_name
                FROM reservations r
                LEFT JOIN users u ON r.user_id = u.user_id
                WHERE LOWER(u.full_name) LIKE LOWER(@name)
                ORDER BY r.date DESC;
                """;

            var reservations = new List<Reservation>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;
            AddParameter(command, "@name", $"%{name}%");

            using var reader = await Task.Run(() => command.ExecuteReader());
            while (reader.Read())
                reservations.Add(MapRowFull(reader));

            return reservations;
        }

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

        /// <inheritdoc />
        public async Task<IEnumerable<Reservation>> GetPaidByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT reservation_code, date, payment_state, user_id, flight_number
                FROM reservations
                WHERE flight_number = @flightNumber
                  AND payment_state = 'Paid'
                ORDER BY date ASC;
            """;

            var reservations = new List<Reservation>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                reservations.Add(MapRow(reader));
            }

            return reservations;
        }

        /// <inheritdoc />
        public async Task<int> GetActiveCountByFlightNumberAsync(string flightNumber)
        {
            const string sql = @"
                SELECT COUNT(*) 
                FROM reservations 
                WHERE flight_number = @FlightNumber 
                  AND payment_state <> @RefundedStatus;";

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            var flightParam = command.CreateParameter();
            flightParam.ParameterName = "@FlightNumber";
            flightParam.Value = flightNumber ?? (object)DBNull.Value;
            command.Parameters.Add(flightParam);

            var statusParam = command.CreateParameter();
            statusParam.ParameterName = "@RefundedStatus";
            statusParam.Value = PaymentStatus.Refunded.ToString();
            command.Parameters.Add(statusParam);

            var result = command.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}
