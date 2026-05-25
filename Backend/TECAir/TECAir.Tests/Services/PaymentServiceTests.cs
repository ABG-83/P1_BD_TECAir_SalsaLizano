using Moq;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;
using TECAir.Core.DTOs.Payments;
using TECAir.Core.Services;

namespace TECAir.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="PaymentService"/>.
    /// Directly mocks underlying structural repository boundaries to enforce transactional business assertions.
    /// </summary>
    public class PaymentServiceTests
    {
        // ── Helpers & Test Data ────────────────────────────────────────────────

        private static Reservation SamplePendingReservation() => new()
        {
            ReservationCode = "TEC-PAY12",
            Date = DateTime.UtcNow,
            PaymentState = PaymentStatus.Pending,
            UserId = 1,
            FlightNumber = "TA-202"
        };

        private static ProcessPaymentDto SampleValidPaymentDto() => new()
        {
            ReservationCode = "TEC-PAY12",
            Amount = 350.50m,
            CardNumber = "4111222233334444", // Mocked valid visa structure pattern
            CardholderName = "Andrés Lizano",
            ExpirationDate = "12/28",
            Cvv = "123"
        };

        // ── ProcessReservationPaymentAsync ─────────────────────────────────────

        [Fact]
        public async Task ProcessReservationPaymentAsync_ValidPayloadAndPendingReservation_ReturnsTrue()
        {
            // Arrange
            var dto = SampleValidPaymentDto();
            var reservation = SamplePendingReservation();

            var mockPaymentRepo = new Mock<IPaymentRepository>();
            var mockReservationRepo = new Mock<IReservationRepository>();

            // Mock finding the target pending booking record
            mockReservationRepo.Setup(r => r.GetByCodeAsync(dto.ReservationCode))
                               .ReturnsAsync(reservation);

            // Mock successfully writing the ledger receipt trace entry
            mockPaymentRepo.Setup(p => p.CreateAsync(It.IsAny<Payment>()))
                           .ReturnsAsync(1);

            // Mock updating the target reservation state to Paid
            mockReservationRepo.Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
                               .ReturnsAsync(true);

            var service = new PaymentService(mockPaymentRepo.Object, mockReservationRepo.Object);

            // Act
            var result = await service.ProcessReservationPaymentAsync(dto);

            // Assert
            Assert.True(result);
            Assert.Equal(PaymentStatus.Paid, reservation.PaymentState); // Ensures memory reference state updated

            mockPaymentRepo.Verify(p => p.CreateAsync(It.Is<Payment>(pay =>
                pay.ReservationCode == dto.ReservationCode &&
                pay.Amount == dto.Amount &&
                pay.PaymentStatus == PaymentStatus.Paid)), Times.Once);

            mockReservationRepo.Verify(r => r.UpdateAsync(It.Is<Reservation>(res =>
                res.ReservationCode == dto.ReservationCode &&
                res.PaymentState == PaymentStatus.Paid)), Times.Once);
        }

        [Fact]
        public async Task ProcessReservationPaymentAsync_NonExistingReservationCode_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = SampleValidPaymentDto();
            dto.ReservationCode = "NOTFOUND";

            var mockPaymentRepo = new Mock<IPaymentRepository>();
            var mockReservationRepo = new Mock<IReservationRepository>();

            // Simulate database returned zero rows for this specific locator token string
            mockReservationRepo.Setup(r => r.GetByCodeAsync("NOTFOUND"))
                               .ReturnsAsync((Reservation?)null);

            var service = new PaymentService(mockPaymentRepo.Object, mockReservationRepo.Object);

            // Act & Assert (Intercepted gracefully downstream by ExceptionMiddleware as 404)
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.ProcessReservationPaymentAsync(dto));

            Assert.Contains("not registered inside the flight manifest", exception.Message.ToLower());
        }

        [Fact]
        public async Task ProcessReservationPaymentAsync_ReservationAlreadyPaid_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = SampleValidPaymentDto();
            var reservation = SamplePendingReservation();
            reservation.PaymentState = PaymentStatus.Paid; // Setting pre-existing cleared status constraint

            var mockPaymentRepo = new Mock<IPaymentRepository>();
            var mockReservationRepo = new Mock<IReservationRepository>();

            mockReservationRepo.Setup(r => r.GetByCodeAsync(dto.ReservationCode))
                               .ReturnsAsync(reservation);

            var service = new PaymentService(mockPaymentRepo.Object, mockReservationRepo.Object);

            // Act & Assert (Intercepted gracefully downstream by ExceptionMiddleware as 400)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ProcessReservationPaymentAsync(dto));

            Assert.Contains("already settled and fully paid", exception.Message.ToLower());
        }

        [Fact]
        public async Task ProcessReservationPaymentAsync_CardDeclinedBySimulatedGateway_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = SampleValidPaymentDto();
            dto.CardNumber = "4000000000000000"; // Triggers the explicit sandbox decline business branch code rule
            var reservation = SamplePendingReservation();

            var mockPaymentRepo = new Mock<IPaymentRepository>();
            var mockReservationRepo = new Mock<IReservationRepository>();

            mockReservationRepo.Setup(r => r.GetByCodeAsync(dto.ReservationCode))
                               .ReturnsAsync(reservation);

            var service = new PaymentService(mockPaymentRepo.Object, mockReservationRepo.Object);

            // Act & Assert (Intercepted gracefully downstream by ExceptionMiddleware as 400)
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ProcessReservationPaymentAsync(dto));

            Assert.Contains("declined by the external transaction clearing", exception.Message.ToLower());

            // Core safety constraint validation: ensure no records are written or altered on bank rejection loops
            mockPaymentRepo.Verify(p => p.CreateAsync(It.IsAny<Payment>()), Times.Never);
            mockReservationRepo.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
        }
    }
}
