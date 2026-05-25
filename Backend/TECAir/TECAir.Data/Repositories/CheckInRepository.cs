// =============================================================================
// Archivo  : CheckInRepository.cs
// Capa     : TECAir.Data → Repositories
// Propósito: Implementación de ICheckInRepository usando ADO.NET puro.
//            Ejecuta consultas directamente sobre la tabla 'check_ins'
//            sin usar ORM ni procedimientos almacenados.
//
//            Flujo de un check-in nuevo:
//              1. IsSeatTakenAsync()        → verificar que el asiento esté libre
//              2. GetByReservationIdAsync() → verificar que no haya check-in previo
//              3. CreateAsync()             → insertar el check-in y obtener el ID
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    public class CheckInRepository(IDbConnectionFactory connectionFactory) : ICheckInRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        // Convierte una fila del DataReader en un objeto CheckIn
        private static CheckIn MapRow(IDataReader r) => new()
        {
            CheckInId = r.GetInt32(r.GetOrdinal("checkin_id")),
            Seat = r.GetString(r.GetOrdinal("seat")),
            BoardingGate = r.GetString(r.GetOrdinal("boarding_gate")),
            PrintTime = r.GetDateTime(r.GetOrdinal("print_time")),
            ReservationCode = r.GetString(r.GetOrdinal("reservation_code")),
            FlightNumber = r.GetString(r.GetOrdinal("flight_number"))
        };

        // Crea y agrega un parámetro con nombre y valor al comando SQL
        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        // ── Comandos ───────────────────────────────────────────────────────────

        // Inserta un nuevo check-in y retorna el ID generado por la BD usando RETURNING
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

            // Parámetros del INSERT — se mapean directamente desde el modelo
            AddParam(command, "seat", checkIn.Seat);
            AddParam(command, "boardingGate", checkIn.BoardingGate);
            AddParam(command, "printTime", checkIn.PrintTime);
            AddParam(command, "reservationCode", checkIn.ReservationCode);
            AddParam(command, "flightNumber", checkIn.FlightNumber);

            // RETURNING checkin_id devuelve el ID recién generado sin hacer un SELECT adicional
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        // ── Queries ────────────────────────────────────────────────────────────

        // Busca un check-in por su ID primario; retorna null si no existe
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

        // Busca el check-in de una reservación específica.
        // Retorna null si el pasajero todavía no ha hecho check-in.
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

        // Retorna todos los check-ins de un vuelo, ordenados por hora de impresión
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

        // Verifica si un asiento ya fue asignado en el vuelo indicado.
        // Usa COUNT(*) para evitar traer datos innecesarios — solo necesitamos saber si existe.
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

            // ExecuteScalar retorna el COUNT; si es mayor a 0 el asiento está ocupado
            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }
}