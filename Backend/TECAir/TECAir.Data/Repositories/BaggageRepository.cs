// =============================================================================
// Archivo  : BaggageRepository.cs
// Capa     : TECAir.Data → Repositories
// Propósito: Implementación de IBaggageRepository usando ADO.NET puro.
//            Ejecuta consultas sobre la tabla 'baggages'.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    public class BaggageRepository(IDbConnectionFactory connectionFactory) : IBaggageRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        // Convierte una fila del DataReader en un objeto Baggage
        private static Baggage MapRow(IDataReader r) => new()
        {
            BaggageId     = r.GetInt32(r.GetOrdinal("baggage_id")),
            Weight        = r.GetDecimal(r.GetOrdinal("weight")),
            Color         = r.GetString(r.GetOrdinal("color")),
            ReservationId = r.GetInt32(r.GetOrdinal("reservation_id")),
            UserId        = r.GetInt32(r.GetOrdinal("user_id"))
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

        
        // Inserta la maleta y recupera el baggage_id generado por SERIAL con RETURNING
        public async Task<int> CreateAsync(Baggage baggage)
        {
            const string sql = """
                INSERT INTO baggages (weight, color, reservation_id, user_id)
                VALUES (@weight, @color, @reservationId, @userId)
                RETURNING baggage_id;
                """;
 
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command    = connection.CreateCommand();
            command.CommandText  = sql;
 
            AddParam(command, "weight",        baggage.Weight);
            AddParam(command, "color",         baggage.Color);
            AddParam(command, "reservationId", baggage.ReservationId);
            AddParam(command, "userId",        baggage.UserId);
 
            // RETURNING devuelve el ID generado directamente como escalar
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        // Retorna todas las maletas asociadas a una reservación
        public async Task<IEnumerable<Baggage>> GetByReservationIdAsync(int reservationId)
        {
            const string sql = """
                SELECT baggage_id, weight, color, reservation_id, user_id
                FROM baggages
                WHERE reservation_id = @reservationId;
                """;

            var baggages = new List<Baggage>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationId", reservationId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                baggages.Add(MapRow(reader));

            return baggages;
        }
        // Busca una maleta por su ID; retorna null si no existe
        public async Task<Baggage?> GetByIdAsync(int baggageId)
        {
            const string sql = """
                SELECT baggage_id, weight, color, reservation_id, user_id
                FROM baggages
                WHERE baggage_id = @baggageId;
                """;
 
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command    = connection.CreateCommand();
            command.CommandText  = sql;
            AddParam(command, "baggageId", baggageId);
 
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        // Cuenta el total de maletas de una reservación usando COUNT en la BD
        // Es más eficiente que traer todas las filas solo para contarlas
        public async Task<int> CountByReservationIdAsync(int reservationId)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM baggages
                WHERE reservation_id = @reservationId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "reservationId", reservationId);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }
    }
}