using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TECAir.Data.Connection
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            var envConnection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            _connectionString = envConnection ?? configuration.GetConnectionString("PostgreConnection")
                ?? throw new ArgumentNullException("No se encontró una configuración de base de datos.");
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
