using TECAir.Data.Interfaces;
using TECAir.Data.Models;
using TECAir.Core.DTOs.Payments;
using TECAir.Core.Interfaces;

namespace TECAir.Core.Services
{
    /// <summary>
    /// Executes financial business logic checks and securely commits payment trace documents to infrastructure stores.
    /// </summary>
    public class PaymentService(
        IPaymentRepository paymentRepository,
        IReservationRepository reservationRepository) : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;

        /// <inheritdoc />
        public async Task<bool> ProcessReservationPaymentAsync(ProcessPaymentDto dto)
        {
            // ── 1. Reservation State Integrity Audit ─────────────────────────
            var reservation = await _reservationRepository.GetByCodeAsync(dto.ReservationCode) ?? throw new KeyNotFoundException($"The target booking reference locator code '{dto.ReservationCode}' is not registered inside the flight manifest database.");

            // Reject processing attempts on documents already balanced and cleared
            if (reservation.PaymentState == PaymentStatus.Paid)
            {
                throw new InvalidOperationException($"The reservation ledger '{dto.ReservationCode}' is already settled and fully paid.");
            }

            // ── 2. External Payment Gateway Sandbox Emulation ────────────────
            // This isolates raw credential parameters from ever hitting disk arrays (PCI-DSS compliance)
            bool externalGatewaySettled = SimulateAcquiringBankClearing(dto.CardNumber, dto.Cvv);
            if (!externalGatewaySettled)
            {
                throw new InvalidOperationException("The card processing transaction was declined by the external transaction clearing framework network.");
            }

            // ── 3. Immutable Receipt Serialization ───────────────────────────
            var receipt = new Payment
            {
                ReservationCode = dto.ReservationCode,
                Amount = dto.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionReference = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                PaymentStatus = PaymentStatus.Paid,
            };

            // Write the financial settlement log entry block to the database structure layer
            await _paymentRepository.CreateAsync(receipt);

            // ── 4. Booking Status Modification Propagation ───────────────────
            // Cascade status changes downward to clear flights allocations
            reservation.PaymentState = PaymentStatus.Paid;
            bool patchSuccess = await _reservationRepository.UpdateAsync(reservation);

            return patchSuccess;
        }

        /// <summary>
        /// Provides a safe environment sandbox loop mocking core transaction checks with card numbers.
        /// </summary>
        private static bool SimulateAcquiringBankClearing(string cardNumber, string cvv)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || string.IsNullOrWhiteSpace(cvv))
                return false;

            // Simple sandbox check rule: prevent execution using test sequence lines
            if (cardNumber.StartsWith("4000000000000000") || cvv == "000")
                return false;

            return true;
        }
    }
}
