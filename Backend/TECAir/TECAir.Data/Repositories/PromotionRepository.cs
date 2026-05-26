// =============================================================================
// File    : PromotionRepository.cs
// Layer   : TECAir.Data → Repositories
// Purpose : Implements promotion persistence logic with ADO.NET.
// =============================================================================

using System.Data;
using TECAir.Data.Connection;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// SQL-backed implementation of <see cref="IPromotionRepository"/> using native ADO.NET.
    /// </summary>
    public class PromotionRepository(IDbConnectionFactory connectionFactory) : IPromotionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        /// <summary>
        /// Maps a data reader row into a <see cref="Promotion"/> domain object.
        /// </summary>
        private static Promotion MapRow(IDataReader r) => new()
        {
            PromotionId = r.GetInt32(r.GetOrdinal("promotion_id")),
            Price = r.GetDecimal(r.GetOrdinal("price")),
            StartDate = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("start_date"))),
            EndDate = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("end_date"))),
            Image = r.IsDBNull(r.GetOrdinal("image")) ? null : r.GetString(r.GetOrdinal("image")),
            IsActive = r.GetBoolean(r.GetOrdinal("is_active")),
            OriginAirportId = r.GetInt32(r.GetOrdinal("origin_airport_id")),
            DestinationAirportId = r.GetInt32(r.GetOrdinal("destination_airport_id")),

            OriginAirportName = r.IsDBNull(r.GetOrdinal("origin_airport_name")) ? null : r.GetString(r.GetOrdinal("origin_airport_name")),
            DestinationAirportName = r.IsDBNull(r.GetOrdinal("destination_airport_name")) ? null : r.GetString(r.GetOrdinal("destination_airport_name"))
        };

        /// <summary>
        /// Creates and adds a command parameter.
        /// </summary>
        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Promotion>> GetAllAsync()
        {
            const string sql = """
                SELECT p.promotion_id, p.price, p.start_date, p.end_date, p.image, p.is_active, 
                    p.origin_airport_id, p.destination_airport_id,
                    a1.name AS origin_airport_name, 
                    a2.name AS destination_airport_name
                FROM promotions p
                LEFT JOIN airports a1 ON p.origin_airport_id = a1.airport_id
                LEFT JOIN airports a2 ON p.destination_airport_id = a2.airport_id
                ORDER BY p.start_date DESC;
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

        /// <inheritdoc />
        public async Task<IEnumerable<Promotion>> GetActiveAsync()
        {
            const string sql = """
                SELECT p.promotion_id, p.price, p.start_date, p.end_date, p.image, p.is_active, 
                    p.origin_airport_id, p.destination_airport_id,
                    a1.name AS origin_airport_name, 
                    a2.name AS destination_airport_name
                FROM promotions p
                LEFT JOIN airports a1 ON p.origin_airport_id = a1.airport_id
                LEFT JOIN airports a2 ON p.destination_airport_id = a2.airport_id
                WHERE p.is_active = TRUE
                ORDER BY p.start_date DESC;
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

        /// <inheritdoc />
        public async Task<Promotion?> GetByIdAsync(int promotionId)
        {
            const string sql = """
                SELECT p.promotion_id, p.price, p.start_date, p.end_date, p.image, p.is_active, 
                    p.origin_airport_id, p.destination_airport_id,
                    a1.name AS origin_airport_name, 
                    a2.name AS destination_airport_name
                FROM promotions p
                LEFT JOIN airports a1 ON p.origin_airport_id = a1.airport_id
                LEFT JOIN airports a2 ON p.destination_airport_id = a2.airport_id
                WHERE p.promotion_id = @promotionId;
                """;

            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParam(command, "promotionId", promotionId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapRow(reader) : null;
        }

        /// <inheritdoc />
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

            AddParam(command, "price", promotion.Price);
            AddParam(command, "startDate", promotion.StartDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "endDate", promotion.EndDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "image", promotion.Image);
            AddParam(command, "isActive", promotion.IsActive);
            AddParam(command, "originAirportId", promotion.OriginAirportId);
            AddParam(command, "destinationAirportId", promotion.DestinationAirportId);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <inheritdoc />
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

            AddParam(command, "price", promotion.Price);
            AddParam(command, "startDate", promotion.StartDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "endDate", promotion.EndDate.ToDateTime(TimeOnly.MinValue));
            AddParam(command, "image", promotion.Image);
            AddParam(command, "isActive", promotion.IsActive);
            AddParam(command, "originAirportId", promotion.OriginAirportId);
            AddParam(command, "destinationAirportId", promotion.DestinationAirportId);
            AddParam(command, "promotionId", promotion.PromotionId);

            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
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
