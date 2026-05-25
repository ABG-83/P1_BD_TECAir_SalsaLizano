using TECAir.Data.Interfaces;
using TECAir.Data.Models;
using TECAir.Core.Interfaces;
using TECAir.Core.DTOs.Reservations;

namespace TECAir.Core.Services
{
    /// <summary>
    /// Implements operations to create, consult, modify, and cancel flight reservations.
    /// </summary>
    public class ReservationService(IReservationRepository reservationRepository, IFlightRepository flightRepository) : IReservationService
    {
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IFlightRepository _flightRepository = flightRepository;

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
            // ── 1. Input Request Data Validation Gate ─────────────────────────
            if (string.IsNullOrWhiteSpace(dto.FlightNumber))
            {
                throw new ArgumentException("The flight number identifier is strictly required to create a booking.");
            }

            if (dto.UserId <= 0)
            {
                throw new ArgumentException("A valid, positive User ID is required to register a reservation owner.");
            }


            // ── 2. Flight Capacity & Seat Availability Validation Gate ────────
            // Fetch maximum passenger structural limit assigned to the target aircraft configuration
            int flightCapacity = await _flightRepository.GetCapacityByFlightNumberAsync(dto.FlightNumber);
            if (flightCapacity <= 0)
            {
                throw new KeyNotFoundException($"The flight schedule '{dto.FlightNumber}' was not found or contains an uninitialized capacity manifest.");
            }

            // Calculate active bookings to prevent overbooking anomalies
            int activeBookingsCount = await _reservationRepository.GetActiveCountByFlightNumberAsync(dto.FlightNumber);
            if (activeBookingsCount >= flightCapacity)
            {
                // Caught by ExceptionMiddleware globally and returned as a standard 400 BadRequest
                throw new InvalidOperationException($"The flight scheme '{dto.FlightNumber}' is completely fully booked. No vacant seats are available.");
            }

            // ── 3. Entity Initial State Construction & Persistence ───────────
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
            // Fetch the existing booking record target to evaluate current status
            var reservation = await _reservationRepository.GetByCodeAsync(reservationCode) ?? throw new KeyNotFoundException($"The reservation tracking code '{reservationCode}' does not exist inside our manifest records.");

            // Target the flight directory data to analyze its real-time operational window state
            var flight = await _flightRepository.GetByFlightNumberAsync(reservation.FlightNumber) ?? throw new KeyNotFoundException($"The associated flight schedule profile '{reservation.FlightNumber}' could not be resolved.");

            // CRITICAL BUSINESS RULE: Block modifications if the flight itinerary is frozen or closed
            // Operational states like Closed, CheckIn, Boarding, or Departed cannot accept variations
            if (flight.Status != FlightStatus.Scheduled)
            {
                throw new InvalidOperationException($"This reservation cannot be modified because the flight '{reservation.FlightNumber}' is currently in a '{flight.Status}' operational phase.");
            }

            // Overwrite memory reference mappings with incoming validated parameters
            reservation.FlightNumber = dto.FlightNumber;
            // (Map any extra fields required by your entity here)

            return await _reservationRepository.UpdateAsync(reservation);
        }

        /// <inheritdoc />
        public async Task<bool> CancelReservationAsync(string reservationCode)
        {
            // Retrieve data context tracking history rows from storage
            var reservation = await _reservationRepository.GetByCodeAsync(reservationCode) ?? throw new KeyNotFoundException($"The reservation code '{reservationCode}' was not found in the travel ledger registry.");

            // Validate that the user is not attempting to double-void an already cancelled or refunded booking
            if (reservation.PaymentState == PaymentStatus.Failed || reservation.PaymentState == PaymentStatus.Refunded)
            {
                throw new InvalidOperationException($"The reservation '{reservationCode}' cannot be cancelled because it is already flagged as void or historically refunded.");
            }

            // Fetch live flight tracking profiles to protect locked boarding slots
            var flight = await _flightRepository.GetByFlightNumberAsync(reservation.FlightNumber);
            if (flight == null)
            {
                throw new KeyNotFoundException($"The underlying flight manifest reference '{reservation.FlightNumber}' does not exist.");
            }

            // CRITICAL BUSINESS RULE: Block cancellations if passenger manifests are already frozen for security
            if (flight.Status == FlightStatus.InAir ||
                flight.Status == FlightStatus.Boarding ||
                flight.Status == FlightStatus.Landed)
            {
                throw new InvalidOperationException($"Cancellation rejected. Passengers registered on flight '{reservation.FlightNumber}' are currently locked due to active security '{flight.Status}' processing clearances.");
            }

            // Proceed to update the financial tracking layout to Cancelled
            return await _reservationRepository.CancelAsync(reservationCode);
        }
    }
}
