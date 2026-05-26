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
            return conn;
        }
    }
}
