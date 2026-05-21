using System.Data;
using Moq;
using TECAir.Data.Connection;
using TECAir.Data.Models;
using TECAir.Data.Repositories;

namespace TECAir.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="AirportRepository"/> using mocked ADO.NET interfaces.
    /// </summary>
    public class AirportRepositoryTests
    {
        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Configures a mocked DataReader to iterate over a specific list of airports.
        /// </summary>
        private static Mock<IDataReader> BuildMockReader(List<Airport> airports)
        {
            var reader = new Mock<IDataReader>();
            var index = -1;

            // Simulates moving through the cursor. Returns true if items remain, false otherwise.
            reader.Setup(r => r.Read()).Returns(() =>
            {
                index++;
                return index < airports.Count;
            });

            // Define rigid numerical positions for each expected column
            const int idIdx = 0;
            const int nameIdx = 1;
            const int locIdx = 2;

            // Simulates the behavior of GetOrdinal("column_name")
            reader.Setup(r => r.GetOrdinal("airport_id")).Returns(idIdx);
            reader.Setup(r => r.GetOrdinal("name")).Returns(nameIdx);
            reader.Setup(r => r.GetOrdinal("location")).Returns(locIdx);

            // Dynamically yields the current item properties based on the active index lookup
            reader.Setup(r => r.GetInt32(idIdx)).Returns(() => airports[index].AirportId);
            reader.Setup(r => r.GetString(nameIdx)).Returns(() => airports[index].Name);
            reader.Setup(r => r.GetString(locIdx)).Returns(() => airports[index].Location);

            return reader;
        }

        /// <summary>
        /// Instantiates a standard test airport model using numeric IDs.
        /// </summary>
        private static Airport SampleAirport(int id = 1, string name = "Juan Santamaría") => new()
        {
            AirportId = id,
            Name = name,
            Location = "Alajuela, Costa Rica"
        };

        // ── GetAllAsync Tests ─────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_WhenAirportsExist_ReturnsListSortedByRepo()
        {
            // Arrange
            var mockData = new List<Airport>
            {
                SampleAirport(1, "Daniel Oduber Quirós"),
                SampleAirport(2, "Juan Santamaría")
            };

            var reader = BuildMockReader(mockData);
            var command = new Mock<IDbCommand>();
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);

            var repo = new AirportRepository(factory.Object);

            // Act
            var result = (await repo.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Daniel Oduber Quirós", result[0].Name);
            Assert.Equal("Juan Santamaría", result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoAirportsExist_ReturnsEmptyList()
        {
            // Arrange
            var reader = BuildMockReader(new List<Airport>()); // Empty dataset simulation
            var command = new Mock<IDbCommand>();
            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);

            var repo = new AirportRepository(factory.Object);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ── GetByIdAsync Tests ────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsMappedAirport()
        {
            // Arrange
            var targetAirport = SampleAirport(7, "Tobías Bolaños");
            var mockData = new List<Airport> { targetAirport };

            var reader = BuildMockReader(mockData);
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);

            var repo = new AirportRepository(factory.Object);

            // Act
            var result = await repo.GetByIdAsync(7);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7, result!.AirportId);
            Assert.Equal("Tobías Bolaños", result.Name);
            Assert.Equal("Alajuela, Costa Rica", result.Location);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var reader = BuildMockReader([]); // Reader immediately returns false on Read()
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);

            var repo = new AirportRepository(factory.Object);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
