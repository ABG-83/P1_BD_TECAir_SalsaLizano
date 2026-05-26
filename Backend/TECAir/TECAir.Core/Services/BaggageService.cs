// =============================================================================
// File    : BaggageService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for baggage operations.
// =============================================================================

using TECAir.Core.DTOs.Baggage;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    public class BaggageService(
        IBaggageRepository baggageRepository,
        IReservationRepository reservationRepository,
        ICheckInRepository checkInRepository,
        IUserRepository userRepository) : IBaggageService
    {
        private readonly IBaggageRepository _baggageRepository = baggageRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly ICheckInRepository _checkInRepository = checkInRepository;
        private readonly IUserRepository _userRepository = userRepository;

        // ── Registro ───────────────────────────────────────────────────────────

        // Validates check-in, calculates the charge, and persists the baggage item.
        public async Task<BaggageResponseDto> AddBaggageAsync(BaggageDto dto)
        {
            // Verify that the reservation exists so the required user_id can be retrieved from the table.
            var reservation = await _reservationRepository.GetByCodeAsync(dto.ReservationCode)
                ?? throw new KeyNotFoundException(
                    $"No existe una reservación con ID {dto.ReservationCode}.");

            // Verificar que el pasajero ya haya hecho check-in (regla del enunciado)
            var checkIn = await _checkInRepository.GetByReservationCodeAsync(dto.ReservationCode);
            if (checkIn is null)
                throw new InvalidOperationException(
                    $"El pasajero de la reservación {dto.ReservationCode} no ha realizado check-in. " +
                    "Solo se pueden registrar maletas a pasajeros ya chequeados.");

            // Count the current baggage items to determine the position of the new one.
            int currentCount = await _baggageRepository.CountByReservationCodeAsync(dto.ReservationCode);
            int newPosition = currentCount + 1;

            // Calculate the additional charge according to the stated rate.
            decimal chargeForThisBag = CalculateChargeForPosition(newPosition);

            // Build and persist the baggage item using the current table data.
            var baggage = new Baggage
            {
                Weight = dto.Weight,
                Color = dto.Color.Trim(),
                ReservationCode = dto.ReservationCode,
                UserId = reservation.UserId
            };

            int newId = await _baggageRepository.CreateAsync(baggage);
            baggage.BaggageId = newId;

            // Resolver el nombre del pasajero para incluirlo en la respuesta
            var user = await _userRepository.GetByIdAsync(reservation.UserId);

            // Calculate the total accumulated charge including the newly registered baggage item.
            decimal totalCharge = CalculateTotalCharge(newPosition);

            return MapToResponseDto(baggage, user?.FullName ?? "Desconocido", newPosition, chargeForThisBag, totalCharge);
        }

        // ── Consultas ──────────────────────────────────────────────────────────

        // Returns all baggage items for a reservation with their individual charge.
        public async Task<IEnumerable<BaggageResponseDto>> GetByReservationCodeAsync(string reservationCode)
        {
            var baggages = await _baggageRepository.GetByReservationCodeAsync(reservationCode);

            // Resolve the passenger name once for the entire baggage list.
            var reservation = await _reservationRepository.GetByCodeAsync(reservationCode);
            var user = reservation is not null
                                ? await _userRepository.GetByIdAsync(reservation.UserId)
                                : null;
            var passengerName = user?.FullName ?? "Desconocido";

            var result = new List<BaggageResponseDto>();
            int position = 1;

            // Assign the position and charge to each baggage item in the list.
            foreach (var bag in baggages)
            {
                decimal chargeForBag = CalculateChargeForPosition(position);
                decimal totalSoFar = CalculateTotalCharge(position);
                result.Add(MapToResponseDto(bag, passengerName, position, chargeForBag, totalSoFar));
                position++;
            }

            return result;
        }

        // Finds a baggage item by ID and resolves its position and charge within the reservation.
        public async Task<BaggageResponseDto?> GetByIdAsync(int baggageId)
        {
            var baggage = await _baggageRepository.GetByIdAsync(baggageId);
            if (baggage is null) return null;

            // Get all baggage items for the reservation to determine the real position.
            var allBags = (await _baggageRepository.GetByReservationCodeAsync(baggage.ReservationCode)).ToList();
            int position = allBags.FindIndex(b => b.BaggageId == baggageId) + 1;

            var reservation = await _reservationRepository.GetByCodeAsync(baggage.ReservationCode);
            var user = reservation is not null
                                ? await _userRepository.GetByIdAsync(reservation.UserId)
                                : null;

            decimal chargeForBag = CalculateChargeForPosition(position);
            decimal totalCharge = CalculateTotalCharge(allBags.Count);

            return MapToResponseDto(baggage, user?.FullName ?? "Desconocido", position, chargeForBag, totalCharge);
        }

        // ── Private helper methods ──────────────────────────────────────────

        // Additional charge for the baggage item in the indicated position: 1st → $0 | 2nd → $50 | 3rd+ → $75
        private static decimal CalculateChargeForPosition(int position) => position switch
        {
            1 => 0m,
            2 => 50m,
            _ => 75m
        };

        // Total accumulated charge for n baggage items: n=3 → $125 | n=5 → $275
        private static decimal CalculateTotalCharge(int totalBags)
        {
            if (totalBags <= 1) return 0m;
            if (totalBags == 2) return 50m;
            return 50m + (75m * (totalBags - 2));
        }

        // Builds the response DTO with the baggage data and the calculated charge.
        private static BaggageResponseDto MapToResponseDto(
            Baggage baggage,
            string passengerName,
            int position,
            decimal chargeForThisBag,
            decimal totalCharge) => new()
            {
                BaggageId = baggage.BaggageId,
                Weight = baggage.Weight,
                Color = baggage.Color,
                ReservationCode = baggage.ReservationCode,
                PassengerName = passengerName,
                BaggagePosition = position,
                ExtraChargeForThisBag = chargeForThisBag,
                TotalExtraCharge = totalCharge
            };
    }
}
