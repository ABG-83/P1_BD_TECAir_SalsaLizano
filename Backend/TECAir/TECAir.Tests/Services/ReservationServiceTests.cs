using Moq;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;
using TECAir.Core.DTOs.Reservations;
using TECAir.Core.Services;

namespace TECAir.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ReservationService"/>.
    /// Mocks data layer abstractions directly to validate core business rule constraints.
    /// </summary>
    public class ReservationServiceTests
    {
        // ── Helpers & Test Data ────────────────────────────────────────────────

        private static Reservation SampleReservation() => new()
        {
            ReservationCode = "TEC-ABC12",
            Date = new DateTime(2026, 05, 25, 12, 00, 00, DateTimeKind.Utc),
            PaymentState = PaymentStatus.Pending,
            UserId = 1,
            FlightNumber = "TA-101"
        };

        private static CreateReservationDto SampleCreateDto() => new()
        {
            UserId = 1,
            FlightNumber = "TA-101"
        };

        // ── CreateReservationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task CreateReservationAsync_ValidDto_ReturnsExpectedResponse()
        {
            // Arrange
            var dto = SampleCreateDto();
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            // Set up repository to mimic returning the saved reservation code locator
            mockFlightRepo.Setup(f => f.GetCapacityByFlightNumberAsync(dto.FlightNumber)).ReturnsAsync(150);
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Reservation>()))
                    .ReturnsAsync("TEC-GEN99");

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act
            var result = await service.CreateReservationAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEC-GEN99", result.ReservationCode);
            Assert.Equal(dto.UserId, result.UserId);
            Assert.Equal(dto.FlightNumber, result.FlightNumber);
            Assert.Equal(PaymentStatus.Pending.ToString(), result.PaymentState);
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task CreateReservationAsync_EmptyFlightNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateReservationDto { UserId = 1, FlightNumber = "" };
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();
            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateReservationAsync(dto));

            Assert.Contains("required", exception.Message.ToLower());
        }

        [Fact]
        public async Task CreateReservationAsync_FlightFullyBooked_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = SampleCreateDto();
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            // Simulate aircraft is at max capacity (e.g., 60 seats capacity and 60 active bookings)
            mockFlightRepo.Setup(f => f.GetCapacityByFlightNumberAsync(dto.FlightNumber)).ReturnsAsync(60);
            mockRepo.Setup(r => r.GetActiveCountByFlightNumberAsync(dto.FlightNumber)).ReturnsAsync(60);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateReservationAsync(dto));

            Assert.Contains("fully booked", exception.Message.ToLower());
            mockRepo.Verify(r => r.CreateAsync(It.IsAny<Reservation>()), Times.Never); // Safety assertion
        }

        // ── GetReservationByCodeAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetReservationByCodeAsync_ExistingCode_ReturnsMappedResponse()
        {
            // Arrange
            var reservation = SampleReservation();
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync(reservation.ReservationCode))
                    .ReturnsAsync(reservation);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act
            var result = await service.GetReservationByCodeAsync(reservation.ReservationCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reservation.ReservationCode, result.ReservationCode);
            Assert.Equal(reservation.FlightNumber, result.FlightNumber);
        }

        [Fact]
        public async Task GetReservationByCodeAsync_NonExistingCode_ReturnsNull()
        {
            // Arrange
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync("INVALID"))
                    .ReturnsAsync((Reservation?)null);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act
            var result = await service.GetReservationByCodeAsync("INVALID");

            // Assert
            Assert.Null(result);
        }

        // ── GetReservationsByUserIdAsync ───────────────────────────────────────

        [Fact]
        public async Task GetReservationsByUserIdAsync_ExistingUserId_ReturnsList()
        {
            // Arrange
            int targetUserId = 1;
            var items = new List<Reservation> { SampleReservation() };
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByUserIdAsync(targetUserId))
                    .ReturnsAsync(items);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act
            var result = (await service.GetReservationsByUserIdAsync(targetUserId)).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(targetUserId, result[0].UserId);
        }

        // ── ModifyReservationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task ModifyReservationAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            string targetCode = "TEC-ABC12";
            var dto = SampleCreateDto();
            var reservation = SampleReservation();

            // NUEVO: Crear un perfil de vuelo válido para pasar la compuerta de estado
            var activeFlight = new Flight { FlightNumber = "TA-101", Status = FlightStatus.Scheduled };

            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).ReturnsAsync(true);

            // NUEVO: Configurar el Mock para que no devuelva null
            mockFlightRepo.Setup(f => f.GetByFlightNumberAsync(reservation.FlightNumber)).ReturnsAsync(activeFlight);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act
            var result = await service.ModifyReservationAsync(targetCode, dto);

            // Assert
            Assert.True(result);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<Reservation>(res => res.FlightNumber == dto.FlightNumber)), Times.Once);
        }

        [Fact]
        public async Task ModifyReservationAsync_NonExistingRecord_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = SampleCreateDto();
            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync("NOTFOUND")).ReturnsAsync((Reservation?)null);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.ModifyReservationAsync("NOTFOUND", dto));
        }

        [Fact]
        public async Task ModifyReservationAsync_FlightIsClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            string targetCode = "TEC-ABC12";
            var dto = SampleCreateDto();
            var reservation = SampleReservation();

            // Setting up a frozen flight state configuration profile constraint
            var closedFlight = new Flight { FlightNumber = "TA-101", Status = FlightStatus.Cancelled };

            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);
            mockFlightRepo.Setup(f => f.GetByFlightNumberAsync(reservation.FlightNumber)).ReturnsAsync(closedFlight);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ModifyReservationAsync(targetCode, dto));

            Assert.Contains("operational phase", exception.Message.ToLower());
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never); // Ensures database was never modified
        }

        // ── CancelReservationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task CancelReservationAsync_ActivePendingReservation_ReturnsTrue()
        {
            // Arrange
            string targetCode = "TEC-ABC12";
            var reservation = SampleReservation();
            reservation.PaymentState = PaymentStatus.Pending;

            // NUEVO: El vuelo debe estar en Scheduled para que se permita la cancelación
            var activeFlight = new Flight { FlightNumber = "TA-101", Status = FlightStatus.Scheduled };

            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);
            mockRepo.Setup(r => r.CancelAsync(targetCode)).ReturnsAsync(true);

            // NUEVO: Alimentar la validación con el mock del vuelo
            mockFlightRepo.Setup(f => f.GetByFlightNumberAsync(reservation.FlightNumber)).ReturnsAsync(activeFlight);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act
            var result = await service.CancelReservationAsync(targetCode);

            // Assert
            Assert.True(result);
            mockRepo.Verify(r => r.CancelAsync(targetCode), Times.Once);
        }

        [Fact]
        public async Task CancelReservationAsync_AlreadyRefundedReservation_ThrowsInvalidOperationException()
        {
            // Arrange
            string targetCode = "TEC-ABC12";
            var reservation = SampleReservation();
            reservation.PaymentState = PaymentStatus.Refunded;

            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act & Assert 
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelReservationAsync(targetCode));

            Assert.Contains("historically refunded", exception.Message.ToLower());
        }

        [Fact]
        public async Task CancelReservationAsync_FlightIsBoarding_ThrowsInvalidOperationException()
        {
            // Arrange
            string targetCode = "TEC-ABC12";
            var reservation = SampleReservation();
            reservation.PaymentState = PaymentStatus.Pending;

            // Setting up a locked boarding tracking gate state rule
            var boardingFlight = new Flight { FlightNumber = "TA-101", Status = FlightStatus.Boarding };

            var mockRepo = new Mock<IReservationRepository>();
            var mockFlightRepo = new Mock<IFlightRepository>();

            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);
            mockFlightRepo.Setup(f => f.GetByFlightNumberAsync(reservation.FlightNumber)).ReturnsAsync(boardingFlight);

            var service = new ReservationService(mockRepo.Object, mockFlightRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelReservationAsync(targetCode));

            Assert.Contains("locked due to active security", exception.Message.ToLower());
            mockRepo.Verify(r => r.CancelAsync(It.IsAny<string>()), Times.Never); // Ensures database was never altered
        }
    }
}
