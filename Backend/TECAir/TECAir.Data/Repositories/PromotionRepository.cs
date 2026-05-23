// =============================================================================
// Archivo  : PromotionRepository.cs
// Capa     : TECAir.Data → Repositories
// Propósito: Implementación de IPromotionRepository usando ADO.NET puro.
//            Ejecuta las operaciones CRUD sobre la tabla 'promotions'.
//
//            Flujo de gestión de promociones:
//              1. GetAllAsync / GetActiveAsync → SELECT para consulta
//              2. GetByIdAsync                 → SELECT para verificar existencia
//              3. CreateAsync                  → INSERT retornando el ID generado
//              4. UpdateAsync                  → UPDATE de todos los campos editables
//              5. DeleteAsync                  → DELETE por ID
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    public class PromotionRepository(IDbConnectionFactory connectionFactory) : IPromotionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        // Convierte una fila del DataReader en un objeto Promotion
        // La columna 'image' es nullable, se usa IsDBNull para evitar excepciones
        private static Promotion MapRow(IDataReader r) => new()
        {
            PromotionId           = r.GetInt32(r.GetOrdinal("promotion_id")),
            Price                 = r.GetDecimal(r.GetOrdinal("price")),
            StartDate             = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("start_date"))),
            EndDate               = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("end_date"))),
            Image                 = r.IsDBNull(r.GetOrdinal("image")) ? null : r.GetString(r.GetOrdinal("image")),
            IsActive              = r.GetBoolean(r.GetOrdinal("is_active")),
            OriginAirportId       = r.GetInt32(r.GetOrdinal("origin_airport_id")),
            DestinationAirportId  = r.GetInt32(r.GetOrdinal("destination_airport_id"))
        };

        // Crea y agrega un parámetro al comando; los valores null se convierten a DBNull
        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        // ── Queries ────────────────────────────────────────────────────────────

        // Retorna todas las promociones ordenadas de más reciente a más antigua
        public async Task<IEnumerable<Promotion>> GetAllAsync()
        {
            const string sql = """
                SELECT promotion_id, price, start_date, end_date, image,
                       is_active, origin_airport_id, destination_airport_id
                FROM promotions
                ORDER BY start_date DESC;
                """;

            var promotions = new List<Promotion>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
                promotions.Add(MapRow(reader));

            return promotions;
        }

        // Filtra solo las promociones activas para la vista de clientes
        public async Task<IEnumerable<Promotion>> GetActiveAsync()
        {
            const string sql = """
                SELECT promotion_id, price, start_date, end_date, image,
                       is_active, origin_airport_id, destination_airport_id
                FROM promotions
                WHERE is_active = TRUE
                ORDER BY start_date DESC;
                """;

            var promotions = new List<Promotion>();
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
                promotions.Add(MapRow(reader));

            return promotions;
        }

        // Busca una promoción por ID; retorna null si no existe
        public async Task<Promotion?> GetByIdAsync(int promotionId)
        {
            const string sql = """
                SELECT promotion_id, price, start_date, end_date, image,
                       is_active, origin_airport_id, destination_airport_id
                FROM promotions
                WHERE promotion_id = @promotionId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "promotionId", promotionId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        // Inserta la promoción y usa RETURNING para obtener el ID generado sin una segunda consulta
        public async Task<int> CreateAsync(Promotion promotion)
        {
            const string sql = """
                INSERT INTO promotions (price, start_date, end_date, image, is_active,
                                        origin_airport_id, destination_airport_id)
                VALUES (@price, @startDate, @endDate, @image, @isActive,
                        @originAirportId, @destinationAirportId)
                RETURNING promotion_id;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "price",               promotion.Price);
            AddParam(command, "startDate",           promotion.StartDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "endDate",             promotion.EndDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "image",               promotion.Image);
            AddParam(command, "isActive",            promotion.IsActive);
            AddParam(command, "originAirportId",     promotion.OriginAirportId);
            AddParam(command, "destinationAirportId",promotion.DestinationAirportId);

            // ExecuteScalar retorna la primera columna de la primera fila, que es el ID generado
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        // Actualiza todos los campos editables de la promoción identificada por su ID
        public async Task UpdateAsync(Promotion promotion)
        {
            const string sql = """
                UPDATE promotions
                SET price                  = @price,
                    start_date             = @startDate,
                    end_date               = @endDate,
                    image                  = @image,
                    is_active              = @isActive,
                    origin_airport_id      = @originAirportId,
                    destination_airport_id = @destinationAirportId
                WHERE promotion_id = @promotionId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            AddParam(command, "price",               promotion.Price);
            AddParam(command, "startDate",           promotion.StartDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "endDate",             promotion.EndDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "image",               promotion.Image);
            AddParam(command, "isActive",            promotion.IsActive);
            AddParam(command, "originAirportId",     promotion.OriginAirportId);
            AddParam(command, "destinationAirportId",promotion.DestinationAirportId);
            AddParam(command, "promotionId",         promotion.PromotionId);

            command.ExecuteNonQuery();
        }

        // Elimina permanentemente el registro de la tabla por su ID
        public async Task DeleteAsync(int promotionId)
        {
            const string sql = """
                DELETE FROM promotions
                WHERE promotion_id = @promotionId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "promotionId", promotionId);

            command.ExecuteNonQuery();
        }
    }
}