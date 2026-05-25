using TECAir.Data.Interfaces;
using TECAir.Data.Models;
using TECAir.Core.Interfaces;
using TECAir.Core.DTOs.Reservations;

namespace TECAir.Core.Services
{
    /// <summary>
    /// Implements operations to create, consult, modify, and cancel flight reservations.
    /// </summary>
    public class ReservationService(IReservationRepository reservationRepository) : IReservationService
    {
        private readonly IReservationRepository _reservationRepository = reservationRepository;

        // ── Helpers ─────────────────────────────────────────────────────
        private static ReservationResponseDto MapToResponse(Reservation model) => new()
        {
            ReservationCode = model.ReservationCode,
            Date = model.Date,
            PaymentState = model.PaymentState.ToString(),
            UserId = model.UserId,
            FlightNumber = model.FlightNumber
        };

        // ── Queries ───────────────────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto)
        {
            // ── Fixed Validation Gate: Evaluate the incoming DTO directly ──
            if (string.IsNullOrWhiteSpace(dto.FlightNumber))
            {
                throw new ArgumentException("The flight number identifier is strictly required to create a booking.");
            }

            if (dto.UserId <= 0)
            {
                throw new ArgumentException("A valid, positive User ID is required to register a reservation owner.");
            }

            var reservation = new Reservation
            {
                ReservationCode = $"TEC-{Guid.NewGuid().ToString()[..5].ToUpper()}",
                Date = DateTime.UtcNow,
                PaymentState = PaymentStatus.Pending,
                UserId = dto.UserId,
                FlightNumber = dto.FlightNumber
            };

            string code = await _reservationRepository.CreateAsync(reservation);
            reservation.ReservationCode = code;

            return MapToResponse(reservation);
        }

        /// <inheritdoc />
        public async Task<ReservationResponseDto?> GetReservationByCodeAsync(string reservationCode)
        {
            var entity = await _reservationRepository.GetByCodeAsync(reservationCode);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsByUserIdAsync(int userId)
        {
            var results = await _reservationRepository.GetByUserIdAsync(userId);
            return results.Select(MapToResponse);
        }

        /// <inheritdoc />
        public async Task<bool> ModifyReservationAsync(string reservationCode, CreateReservationDto dto)
        {
            var existing = await _reservationRepository.GetByCodeAsync(reservationCode) ?? throw new KeyNotFoundException($"No reservation record found matching code target: {reservationCode}");

            // Bind values keeping native identifier code and status secure
            existing.UserId = dto.UserId;
            existing.FlightNumber = dto.FlightNumber;

            return await _reservationRepository.UpdateAsync(existing);
        }

        /// <inheritdoc />
        public async Task<bool> CancelReservationAsync(string reservationCode)
        {
            var existing = await _reservationRepository.GetByCodeAsync(reservationCode) ?? throw new KeyNotFoundException($"The reservation locator code '{reservationCode}' does not exist.");
            if (existing.PaymentState == PaymentStatus.Failed || existing.PaymentState == PaymentStatus.Refunded)
            {
                throw new InvalidOperationException("This reservation cannot be cancelled because it is already void.");
            }

            return await _reservationRepository.CancelAsync(reservationCode);
        }
    }
}
