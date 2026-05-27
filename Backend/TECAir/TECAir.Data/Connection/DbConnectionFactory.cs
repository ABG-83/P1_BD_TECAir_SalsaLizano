// =============================================================================
// File    : DbConnectionFactory.cs
// Layer   : TECAir.Data → Connection
// Purpose : Creates and manages database connections for the data layer.
// =============================================================================

using System.Data;
using Npgsql;

namespace TECAir.Data.Connection
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> CreateConnectionAsync();
    }

    public class DbConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        private readonly string _connectionString = connectionString;

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            // Force UTF-8 client encoding so PostgreSQL converts any stored data
            // to UTF-8 before sending it to the application, regardless of the
            // database-level encoding (common issue on Windows where PostgreSQL
            // defaults to WIN1252).
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SET client_encoding TO 'UTF8'";
            await cmd.ExecuteNonQueryAsync();
            return conn;
        }
    }
}
