// =============================================================================
// File    : CheckInRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements check-in persistence logic with ADO.NET.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="ICheckInRepository"/> using native ADO.NET.
    /// </summary>
    public class CheckInRepository(IDbConnectionFactory connectionFactory) : ICheckInRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Maps a data reader row into a <see cref="CheckIn"/> domain object.
        /// </summary>
        private static CheckIn MapRow(IDataReader r) => new()
        {
            CheckInId = r.GetInt32(r.GetOrdinal("checkin_id")),
            Seat = r.GetString(r.GetOrdinal("seat")),
            BoardingGate = r.GetString(r.GetOrdinal("boarding_gate")),
            PrintTime = r.GetDateTime(r.GetOrdinal("print_time")),
            ReservationCode = r.GetString(r.GetOrdinal("reservation_code")),
            FlightNumber = r.GetString(r.GetOrdinal("flight_number"))
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
        public async Task<int> CreateAsync(CheckIn checkIn)
        {
            const string sql = """
                INSERT INTO check_ins (seat, boarding_gate, print_time, reservation_code, flight_number)
                VALUES (@seat, @boardingGate, @printTime, @reservationCode, @flightNumber)
                RETURNING checkin_id;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "seat", checkIn.Seat);
            AddParam(command, "boardingGate", checkIn.BoardingGate);
            AddParam(command, "printTime", checkIn.PrintTime);
            AddParam(command, "reservationCode", checkIn.ReservationCode);
            AddParam(command, "flightNumber", checkIn.FlightNumber);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <inheritdoc />
        public async Task<CheckIn?> GetByIdAsync(int checkInId)
        {
            const string sql = """
                SELECT checkin_id, seat, boarding_gate, print_time, reservation_code, flight_number
                FROM check_ins
                WHERE checkin_id = @checkInId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "checkInId", checkInId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        /// <inheritdoc />
        public async Task<CheckIn?> GetByReservationCodeAsync(string reservationCode)
        {
            const string sql = """
                SELECT checkin_id, seat, boarding_gate, print_time, reservation_code, flight_number
                FROM check_ins
                WHERE reservation_code = @reservationCode;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationCode", reservationCode);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CheckIn>> GetByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT checkin_id, seat, boarding_gate, print_time, reservation_code, flight_number
                FROM check_ins
                WHERE flight_number = @flightNumber
                ORDER BY print_time ASC;
                """;

            var checkIns = new List<CheckIn>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                checkIns.Add(MapRow(reader));

            return checkIns;
        }

        /// <inheritdoc />
        public async Task<int> GetCountByReservationCodeAsync(string reservationCode)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM check_ins
                WHERE reservation_code = @reservationCode;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationCode", reservationCode);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count;
        }

        /// <inheritdoc />
        public async Task<bool> IsSeatTakenAsync(string flightNumber, string seat)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM check_ins
                WHERE flight_number = @flightNumber
                  AND UPPER(seat) = UPPER(@seat);
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "flightNumber", flightNumber);
            AddParam(command, "seat", seat);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }
}
