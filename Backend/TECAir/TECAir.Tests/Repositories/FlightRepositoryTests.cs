using System.Data;
using Moq;
using TECAir.Data.Connection;
using TECAir.Data.Models;
using TECAir.Data.Repositories;

namespace TECAir.Tests.Repositories
{
    /// <summary>
    /// Unit tests for <see cref="FlightRepository"/>.
    /// The database connection is fully mocked — no real Postgres required.
    /// </summary>
    public class FlightRepositoryTests
    {
        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a mock IDataReader that returns a sequence of flights.
        /// </summary>
        private static Mock<IDataReader> BuildReaderForFlights(List<Flight> flights)
        {
            var reader = new Mock<IDataReader>();
            var index = -1;

            // Simulates the while(reader.Read()) loop moving through the list
            reader.Setup(r => r.Read()).Returns(() =>
            {
                index++;
                return index < flights.Count;
            });

            // Define physical column indexes for mapping simulation
            const int numberIdx = 0;
            const int departureIdx = 1;
            const int arrivalIdx = 2;
            const int statusIdx = 3;
            const int plateIdx = 4;
            const int originIdx = 5;
            const int destIdx = 6;

            // Map exact string column names used in your MapRow to indexes
            reader.Setup(r => r.GetOrdinal("flight_number")).Returns(numberIdx);
            reader.Setup(r => r.GetOrdinal("departure_time")).Returns(departureIdx);
            reader.Setup(r => r.GetOrdinal("arrival_time")).Returns(arrivalIdx);
            reader.Setup(r => r.GetOrdinal("status")).Returns(statusIdx);
            reader.Setup(r => r.GetOrdinal("airplane_plate_number")).Returns(plateIdx);
            reader.Setup(r => r.GetOrdinal("origin_airport_id")).Returns(originIdx);
            reader.Setup(r => r.GetOrdinal("destination_airport_id")).Returns(destIdx);

            // Configure readers to pull dynamically from the current flight in the loop
            reader.Setup(r => r.GetString(numberIdx)).Returns(() => flights[index].FlightNumber);
            reader.Setup(r => r.GetDateTime(departureIdx)).Returns(() => flights[index].DepartureTime);
            reader.Setup(r => r.GetDateTime(arrivalIdx)).Returns(() => flights[index].ArrivalTime);
            reader.Setup(r => r.GetString(statusIdx)).Returns(() => flights[index].Status.ToString());
            reader.Setup(r => r.GetString(plateIdx)).Returns(() => flights[index].AirplanePlateNumber);
            reader.Setup(r => r.GetInt32(originIdx)).Returns(() => flights[index].OriginAirportId);
            reader.Setup(r => r.GetInt32(destIdx)).Returns(() => flights[index].DestinationAirportId);

            return reader;
        }

        /// <summary>Builds an empty mock IDataReader (no rows).</summary>
        private static Mock<IDataReader> BuildEmptyReader()
        {
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(false);
            return reader;
        }

        // ── GetFlightsByRouteAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetFlightsByRouteAsync_MatchingRoute_ReturnsFlightsSorted()
        {
            // Arrange
            int originId = 1;      // e.g., SJO
            int destinationId = 2; // e.g., MIA

            var expectedFlights = new List<Flight>
            {
                new() {
                    FlightNumber = "TA-101",
                    DepartureTime = new DateTime(2026, 06, 15, 08, 00, 00, DateTimeKind.Utc),
                    ArrivalTime = new DateTime(2026, 06, 15, 12, 00, 00, DateTimeKind.Utc),
                    Status = FlightStatus.Scheduled, // Ajustalo al casing real de tu enum (e.g. Scheduled o SCHEDULED)
                    AirplanePlateNumber = "TI-TEC1",
                    OriginAirportId = originId,
                    DestinationAirportId = destinationId
                },
                new() {
                    FlightNumber = "TA-102",
                    DepartureTime = new DateTime(2026, 06, 15, 14, 30, 00, DateTimeKind.Utc),
                    ArrivalTime = new DateTime(2026, 06, 15, 18, 30, 00, DateTimeKind.Utc),
                    Status = FlightStatus.Scheduled,
                    AirplanePlateNumber = "TI-TEC2",
                    OriginAirportId = originId,
                    DestinationAirportId = destinationId
                }
            };

            var reader = BuildReaderForFlights(expectedFlights);
            var command = new Mock<IDbCommand>();
            var parameters = new Mock<IDataParameterCollection>();

            command.Setup(c => c.ExecuteReader()).Returns(reader.Object);
            command.Setup(c => c.Parameters).Returns(parameters.Object);
            command.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);

            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.CreateCommand()).Returns(command.Object);

            var factory = new Mock<IDbConnectionFactory>();
            factory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);

            var repo = new FlightRepository(factory.Object);

            // Act
            var result = (await repo.GetFlightsByRouteAsync(originId, destinationId)).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Verify first flight mapping data
            Assert.Equal("TA-101", result[0].FlightNumber);
            Assert.Equal("TI-TEC1", result[0].AirplanePlateNumber);
            Assert.Equal(originId, result[0].OriginAirportId);
            Assert.Equal(destinationId, result[0].DestinationAirportId);

            // Verify second flight mapping data
            Assert.Equal("TA-102", result[1].FlightNumber);
            Assert.Equal("TI-TEC2", result[1].AirplanePlateNumber);
        }

        [Fact]
        public async Task GetFlightsByRouteAsync_NoFlightsFound_ReturnsEmptyEnumerable()
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
            factory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);

            var repo = new FlightRepository(factory.Object);

            // Act
            var result = await repo.GetFlightsByRouteAsync(999, 888);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
