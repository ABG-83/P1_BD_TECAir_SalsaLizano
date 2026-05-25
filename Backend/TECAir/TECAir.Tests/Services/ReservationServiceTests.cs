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

            // Set up repository to mimic returning the saved reservation code locator
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<Reservation>()))
                    .ReturnsAsync("TEC-GEN99");

            var service = new ReservationService(mockRepo.Object);

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
            // Ensure the input payload explicitly hits the whitespace validation gate
            var dto = new CreateReservationDto { UserId = 1, FlightNumber = "" };
            var mockRepo = new Mock<IReservationRepository>();
            var service = new ReservationService(mockRepo.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateReservationAsync(dto));

            Assert.Contains("required", exception.Message.ToLower());
        }

        // ── GetReservationByCodeAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetReservationByCodeAsync_ExistingCode_ReturnsMappedResponse()
        {
            // Arrange
            var reservation = SampleReservation();
            var mockRepo = new Mock<IReservationRepository>();
            mockRepo.Setup(r => r.GetByCodeAsync(reservation.ReservationCode))
                    .ReturnsAsync(reservation);

            var service = new ReservationService(mockRepo.Object);

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
            mockRepo.Setup(r => r.GetByCodeAsync("INVALID"))
                    .ReturnsAsync((Reservation?)null);

            var service = new ReservationService(mockRepo.Object);

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
            mockRepo.Setup(r => r.GetByUserIdAsync(targetUserId))
                    .ReturnsAsync(items);

            var service = new ReservationService(mockRepo.Object);

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

            var mockRepo = new Mock<IReservationRepository>();
            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).ReturnsAsync(true);

            var service = new ReservationService(mockRepo.Object);

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
            mockRepo.Setup(r => r.GetByCodeAsync("NOTFOUND")).ReturnsAsync((Reservation?)null);

            var service = new ReservationService(mockRepo.Object);

            // Act & Assert (Intercepted by ExceptionMiddleware as 404 NotFound)
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.ModifyReservationAsync("NOTFOUND", dto));
        }

        // ── CancelReservationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task CancelReservationAsync_ActivePendingReservation_ReturnsTrue()
        {
            // Arrange
            string targetCode = "TEC-ABC12";
            var reservation = SampleReservation();
            reservation.PaymentState = PaymentStatus.Pending; // Active valid booking state

            var mockRepo = new Mock<IReservationRepository>();
            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);
            mockRepo.Setup(r => r.CancelAsync(targetCode)).ReturnsAsync(true);

            var service = new ReservationService(mockRepo.Object);

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
            mockRepo.Setup(r => r.GetByCodeAsync(targetCode)).ReturnsAsync(reservation);

            var service = new ReservationService(mockRepo.Object);

            // Act & Assert 
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelReservationAsync(targetCode));

            // Fixed substring requirement to align with actual ExceptionMiddleware payload string
            Assert.Contains("already void", exception.Message.ToLower());
        }
    }
}
