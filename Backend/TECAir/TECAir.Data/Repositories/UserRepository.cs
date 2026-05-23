using TECAir.Data.Models;
using TECAir.Data.Interfaces;
using Npgsql;
using TECAir.Data.Connection;
using System.Data;
using NpgsqlTypes;

namespace TECAir.Data.Repositories
{
    /// <summary>
    /// Handles all database operations for <see cref="User"/> using raw Npgsql.
    /// </summary>
    public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Maps a data-reader row to a <see cref="User"/> instance.</summary>
        private static User MapRow(IDataReader r) => new()
        {
            UserId = r.GetInt32(r.GetOrdinal("user_id")),
            FullName = r.GetString(r.GetOrdinal("full_name")),
            Email = r.GetString(r.GetOrdinal("email")),
            PhoneNumber = r.GetString(r.GetOrdinal("phone_number")),
            Role = Enum.Parse<UserRole>(r.GetString(r.GetOrdinal("role")), ignoreCase: true),
            Miles = r.GetFloat(r.GetOrdinal("miles")),
            CollegeIdNumber = r.GetString(r.GetOrdinal("college_id_number")),
            College = r.GetString(r.GetOrdinal("college"))
        };

        /// <summary>Creates and adds a plain IDbDataParameter to a command.</summary>
        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        /// <summary>
        /// Adds the role parameter with the correct Postgres ENUM cast.
        /// Falls back to plain string when the command is not a real NpgsqlCommand (e.g. in tests).
        /// </summary>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
            AddParam(cmd, "email", user.Email);
            AddParam(cmd, "phone", user.PhoneNumber);
            AddRoleParam(cmd, "role", user.Role);
            AddParam(cmd, "miles", user.Miles);
            AddParam(cmd, "collegeId", user.CollegeIdNumber);
            AddParam(cmd, "college", user.College);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(User user)
        {
            const string sql = """
                UPDATE users
                SET full_name        = @fullName,
                    phone_number     = @phone,
                    college_id_number = @collegeId,
                    college          = @college
                WHERE user_id = @userId;
                """;

            using var conn = await _connectionFactory.CreateConnectionAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "fullName", user.FullName);
            AddParam(cmd, "phone", user.PhoneNumber);
            AddParam(cmd, "collegeId", user.CollegeIdNumber);
            AddParam(cmd, "college", user.College);
            AddParam(cmd, "userId", user.UserId);

            return cmd.ExecuteNonQuery() > 0;
        }

        /// <inheritdoc/>
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
