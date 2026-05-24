// =============================================================================
// Archivo  : ReservationRepository.cs
// Capa     : TECAir.Data → Repositories
// Propósito: Implementación de IReservationRepository usando ADO.NET puro.
//            Ejecuta consultas sobre la tabla 'reservations'.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    public class ReservationRepository(IDbConnectionFactory connectionFactory) : IReservationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        // Convierte una fila del DataReader en un objeto Reservation
        private static Reservation MapRow(IDataReader r) => new()
        {
            ReservationId = r.GetInt32(r.GetOrdinal("reservation_id")),
            Date          = r.GetDateTime(r.GetOrdinal("date")),
            PaymentStatus = r.GetString(r.GetOrdinal("payment_status")),
            UserId        = r.GetInt32(r.GetOrdinal("user_id")),
            FlightNumber  = r.GetString(r.GetOrdinal("flight_number"))
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

        // Retorna todas las reservaciones de un vuelo sin filtrar por estado de pago
        public async Task<IEnumerable<Reservation>> GetByFlightNumberAsync(string flightNumber)
        {
            const string sql = """
                SELECT reservation_id, date, payment_status, user_id, flight_number
                FROM reservations
                WHERE flight_number = @flightNumber
                ORDER BY date ASC;
                """;

            var reservations = new List<Reservation>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                reservations.Add(MapRow(reader));

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
            AddParam(command, "flightNumber", flightNumber);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                reservations.Add(MapRow(reader));

            return reservations;
        }

        // Busca una reservación por su ID; retorna null si no existe
        public async Task<Reservation?> GetByIdAsync(int reservationId)
        {
            const string sql = """
                SELECT reservation_id, date, payment_status, user_id, flight_number
                FROM reservations
                WHERE reservation_id = @reservationId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationId", reservationId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }
    }
}