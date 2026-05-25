// =============================================================================
// Archivo  : UserRepository.cs
// Capa     : TECAir.Data → Repositories
// Propósito: Implementación de IUserRepository usando ADO.NET puro contra
//            la tabla 'users' en PostgreSQL.
//
// Historial de cambios:
//   Issue #29 — Apertura de Vuelos:
//     Fix en MapRow: college_id_number y college son nullables en la BD.
//     Usuarios no estudiantes tienen NULL en esos campos, lo que causaba
//     una excepción al llamar GetString sin verificar IsDBNull primero.
// =============================================================================

using TECAir.Data.Models;
using TECAir.Data.Interfaces;
using Npgsql;
using TECAir.Data.Connection;
using System.Data;
using NpgsqlTypes;

namespace TECAir.Data.Repositories
{
    public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        // Convierte una fila del DataReader en un objeto User
        // Los campos college_id_number y college son nullables: usuarios no estudiantes los tienen en NULL
        private static User MapRow(IDataReader r) => new()
        {
            UserId          = r.GetInt32(r.GetOrdinal("user_id")),
            FullName        = r.GetString(r.GetOrdinal("full_name")),
            Email           = r.GetString(r.GetOrdinal("email")),
            PhoneNumber     = r.GetString(r.GetOrdinal("phone_number")),
            Role            = Enum.Parse<UserRole>(r.GetString(r.GetOrdinal("role")), ignoreCase: true),
            Miles           = r.GetFloat(r.GetOrdinal("miles")),
            // Se verifica DBNull antes de leer para evitar excepciones con usuarios no estudiantes
            CollegeIdNumber = r.IsDBNull(r.GetOrdinal("college_id_number")) ? null : r.GetString(r.GetOrdinal("college_id_number")),
            College         = r.IsDBNull(r.GetOrdinal("college"))           ? null : r.GetString(r.GetOrdinal("college"))
        };

        // Crea y agrega un parámetro al comando
        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        // El rol se almacena como tipo ENUM en PostgreSQL, requiere NpgsqlDbType.Unknown para el cast correcto
        private static void AddRoleParam(IDbCommand cmd, string name, UserRole role)
        {
            if (cmd is NpgsqlCommand npgsqlCmd)
            {
                npgsqlCmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Unknown)
                {
                    Value = role.ToString()
                });
            }
            else
            {
                AddParam(cmd, name, role.ToString());
            }
        }

        // ── Queries ────────────────────────────────────────────────────────────

        // Retorna todos los usuarios ordenados por ID
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            const string sql = """
                SELECT user_id, full_name, email, phone_number,
                       role, miles, college_id_number, college
                FROM users
                ORDER BY user_id;
                """;

            var users = new List<User>();
            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                users.Add(MapRow(rdr));

            return users;
        }

        // Busca un usuario por su ID; retorna null si no existe
        public async Task<User?> GetByIdAsync(int userId)
        {
            const string sql = """
                SELECT user_id, full_name, email, phone_number,
                       role, miles, college_id_number, college
                FROM users
                WHERE user_id = @userId;
                """;

            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "userId", userId);

            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? MapRow(rdr) : null;
        }

        // Busca un usuario por su correo electrónico; retorna null si no existe
        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = """
                SELECT user_id, full_name, email, phone_number,
                       role, miles, college_id_number, college
                FROM users
                WHERE email = @email;
                """;

            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "email", email);

            using var rdr = cmd.ExecuteReader();
            return rdr.Read() ? MapRow(rdr) : null;
        }

        // Inserta un nuevo usuario y retorna el ID auto-generado
        public async Task<int> CreateAsync(User user)
        {
            const string sql = """
                INSERT INTO users (full_name, email, phone_number, role,
                                miles, college_id_number, college)
                VALUES (@fullName, @email, @phone, @role,
                        @miles, @collegeId, @college)
                RETURNING user_id;
                """;

            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "fullName", user.FullName);
            AddParam(cmd, "email",    user.Email);
            AddParam(cmd, "phone",    user.PhoneNumber);
            AddRoleParam(cmd, "role", user.Role);
            AddParam(cmd, "miles",    user.Miles);
            AddParam(cmd, "collegeId",user.CollegeIdNumber ?? (object)DBNull.Value);
            AddParam(cmd, "college",  user.College         ?? (object)DBNull.Value);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        // Actualiza los campos editables de un usuario existente
        public async Task<bool> UpdateAsync(User user)
        {
            const string sql = """
                UPDATE users
                SET full_name         = @fullName,
                    phone_number      = @phone,
                    college_id_number = @collegeId,
                    college           = @college
                WHERE user_id = @userId;
                """;

            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "fullName", user.FullName);
            AddParam(cmd, "phone",    user.PhoneNumber);
            AddParam(cmd, "collegeId",user.CollegeIdNumber ?? (object)DBNull.Value);
            AddParam(cmd, "college",  user.College         ?? (object)DBNull.Value);
            AddParam(cmd, "userId",   user.UserId);

            return cmd.ExecuteNonQuery() > 0;
        }

        // Elimina un usuario por su ID; retorna true si se eliminó correctamente
        public async Task<bool> DeleteAsync(int userId)
        {
            const string sql = "DELETE FROM users WHERE user_id = @userId;";

            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "userId", userId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}