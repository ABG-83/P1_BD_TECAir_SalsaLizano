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