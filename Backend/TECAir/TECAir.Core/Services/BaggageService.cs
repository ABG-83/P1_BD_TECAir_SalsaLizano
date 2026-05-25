// =============================================================================
// Archivo  : BaggageService.cs
// Capa     : TECAir.Core → Services
// Propósito: Implementa la lógica de negocio para el registro de maletas.
//            Coordina tres repositorios: baggage, reservations y check-ins.
//
//            Reglas de negocio aplicadas :
//              - Solo se puede asignar maletas a un pasajero ya chequeado;
//                se verifica buscando un check-in activo para la reservación
//              - Tarifa de cobro adicional por maleta:
//                  Posición 1 → $0.00   (gratis)
//                  Posición 2 → $50.00
//                  Posición 3+ → $75.00 por cada una
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

        // Verifica check-in, calcula cobro y persiste la maleta
        public async Task<BaggageResponseDto> AddBaggageAsync(BaggageDto dto)
        {
            // Verificar que la reservación exista para obtener el user_id requerido por la tabla
            var reservation = await _reservationRepository.GetByCodeAsync(dto.ReservationCode)
                ?? throw new KeyNotFoundException(
                    $"No existe una reservación con ID {dto.ReservationCode}.");

            // Verificar que el pasajero ya haya hecho check-in (regla del enunciado)
            var checkIn = await _checkInRepository.GetByReservationCodeAsync(dto.ReservationCode);
            if (checkIn is null)
                throw new InvalidOperationException(
                    $"El pasajero de la reservación {dto.ReservationCode} no ha realizado check-in. " +
                    "Solo se pueden registrar maletas a pasajeros ya chequeados.");

            // Contar las maletas actuales para determinar la posición de la nueva
            int currentCount = await _baggageRepository.CountByReservationCodeAsync(dto.ReservationCode);
            int newPosition = currentCount + 1;

            // Calcular el cargo adicional según la tarifa del enunciado
            decimal chargeForThisBag = CalculateChargeForPosition(newPosition);

            // Construir y persistir la maleta con los datos de la tabla existente
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

            // Calcular el total acumulado incluyendo la maleta recién registrada
            decimal totalCharge = CalculateTotalCharge(newPosition);

            return MapToResponseDto(baggage, user?.FullName ?? "Desconocido", newPosition, chargeForThisBag, totalCharge);
        }

        // ── Consultas ──────────────────────────────────────────────────────────

        // Retorna todas las maletas de una reservación con su cobro individual
        public async Task<IEnumerable<BaggageResponseDto>> GetByReservationCodeAsync(string reservationCode)
        {
            var baggages = await _baggageRepository.GetByReservationCodeAsync(reservationCode);

            // Resolver el nombre del pasajero una sola vez para todas las maletas
            var reservation = await _reservationRepository.GetByCodeAsync(reservationCode);
            var user = reservation is not null
                                ? await _userRepository.GetByIdAsync(reservation.UserId)
                                : null;
            var passengerName = user?.FullName ?? "Desconocido";

            var result = new List<BaggageResponseDto>();
            int position = 1;

            // Asignar posición y cobro a cada maleta de la lista
            foreach (var bag in baggages)
            {
                decimal chargeForBag = CalculateChargeForPosition(position);
                decimal totalSoFar = CalculateTotalCharge(position);
                result.Add(MapToResponseDto(bag, passengerName, position, chargeForBag, totalSoFar));
                position++;
            }

            return result;
        }

        // Busca una maleta por ID y resuelve su posición y cobro dentro de la reservación
        public async Task<BaggageResponseDto?> GetByIdAsync(int baggageId)
        {
            var baggage = await _baggageRepository.GetByIdAsync(baggageId);
            if (baggage is null) return null;

            // Obtener todas las maletas de la reservación para determinar la posición real
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

        // ── Métodos privados de apoyo ──────────────────────────────────────────

        // Cargo adicional para la maleta en la posición indicada: 1ra→$0 | 2da→$50 | 3ra+→$75
        private static decimal CalculateChargeForPosition(int position) => position switch
        {
            1 => 0m,
            2 => 50m,
            _ => 75m
        };

        // Total acumulado para n maletas: n=3→$125  |  n=5→$275
        private static decimal CalculateTotalCharge(int totalBags)
        {
            if (totalBags <= 1) return 0m;
            if (totalBags == 2) return 50m;
            return 50m + (75m * (totalBags - 2));
        }

        // Construye el DTO de respuesta con los datos de la maleta y su cobro calculado
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