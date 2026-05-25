using System.Data;
using Moq;
using TECAir.Data.Connection;
using TECAir.Data.Models;
using TECAir.Data.Repositories;

namespace TECAir.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="UserRepository"/>.
    /// The database connection is fully mocked — no real Postgres required.
    /// </summary>
    public class UserRepositoryTests
    {
        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a mock IDataReader that returns exactly one row with the
        /// given <paramref name="user"/> data, then returns false on the second Read().
        /// </summary>
        private static Mock<IDataReader> BuildReaderForUser(User user)
        {
            var reader = new Mock<IDataReader>();
            var callCount = 0;

            // First call to ReadAsync returns true (row exists), second returns false (no more rows)
            reader.Setup(r => r.Read()).Returns(() => ++callCount == 1);

            // Define arbitrary numerical positions for each expected column
            const int idIdx = 0;
            const int nameIdx = 1;
            const int emailIdx = 2;
            const int phoneIdx = 3;
            const int roleIdx = 4;
            const int milesIdx = 5;
            const int collegeIdIdx = 6;
            const int collegeIdx = 7;

            // Map column names to their respective mock indexes (simulating GetOrdinal behavior)
            reader.Setup(r => r.GetOrdinal("user_id")).Returns(idIdx);
            reader.Setup(r => r.GetOrdinal("full_name")).Returns(nameIdx);
            reader.Setup(r => r.GetOrdinal("email")).Returns(emailIdx);
            reader.Setup(r => r.GetOrdinal("phone_number")).Returns(phoneIdx);
            reader.Setup(r => r.GetOrdinal("role")).Returns(roleIdx);
            reader.Setup(r => r.GetOrdinal("miles")).Returns(milesIdx);
            reader.Setup(r => r.GetOrdinal("college_id_number")).Returns(collegeIdIdx);
            reader.Setup(r => r.GetOrdinal("college")).Returns(collegeIdx);

            // Configure typed data readers to return the correct mock property based on index lookup
            reader.Setup(r => r.GetInt32(idIdx)).Returns(user.UserId);
            reader.Setup(r => r.GetString(nameIdx)).Returns(user.FullName);
            reader.Setup(r => r.GetString(emailIdx)).Returns(user.Email);
            reader.Setup(r => r.GetString(phoneIdx)).Returns(user.PhoneNumber);
            reader.Setup(r => r.GetString(roleIdx)).Returns(user.Role.ToString());
            reader.Setup(r => r.GetFloat(milesIdx)).Returns(user.Miles);
            reader.Setup(r => r.GetString(collegeIdIdx)).Returns(user.CollegeIdNumber ?? string.Empty);
            reader.Setup(r => r.GetString(collegeIdx)).Returns(user.College ?? string.Empty);

            return reader;
        }

        /// <summary>Builds an empty mock IDataReader (no rows).</summary>
        private static Mock<IDataReader> BuildEmptyReader()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            return reader;
        }

        /// <summary>Sample user reused across tests.</summary>
        private static User SampleUser() => new()
        {
            UserId = 1,
            FullName = "Carlos Méndez",
            Email = "carlos.mendez@estudiantec.cr",
            PhoneNumber = "+50688887777",
            Role = UserRole.CLIENT,
            Miles = 150f,
            CollegeIdNumber = "2021123456",
            College = "Instituto Tecnológico de Costa Rica"
        };

        // ── GetByIdAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsUser()
        {
            // Arrange
            var user = SampleUser();
            var reader = BuildReaderForUser(user);
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.GetByIdAsync(user.UserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var reader = BuildEmptyReader();
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ── GetByEmailAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Arrange
            var user = SampleUser();
            var reader = BuildReaderForUser(user);
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.GetByEmailAsync(user.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Arrange
            var reader = BuildEmptyReader();
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.GetByEmailAsync("noexiste@test.com");

            // Assert
            Assert.Null(result);
        }

        // ── UpdateAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ExistingUser_ReturnsTrue()
        {
            // Arrange — ExecuteNonQuery returns 1 (one row affected)
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteNonQuery()).Returns(1);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.UpdateAsync(SampleUser());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingUser_ReturnsFalse()
        {
            // Arrange — ExecuteNonQuery returns 0 (no rows affected)
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteNonQuery()).Returns(0);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.UpdateAsync(SampleUser());

            // Assert
            Assert.False(result);
        }

        // ── DeleteAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ExistingUser_ReturnsTrue()
        {
            // Arrange
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteNonQuery()).Returns(1);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingUser_ReturnsFalse()
        {
            // Arrange
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteNonQuery()).Returns(0);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var result = await repo.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        // ── CreateAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidUser_ReturnsGeneratedId()
        {
            // Arrange — ExecuteScalar returns the new user_id
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteScalar()).Returns(42);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync())
                   .ReturnsAsync(connection.Object);

            var repo = new UserRepository(factory.Object);

            // Act
            var newId = await repo.CreateAsync(SampleUser());

            // Assert
            Assert.Equal(42, newId);
        }
    }
}
