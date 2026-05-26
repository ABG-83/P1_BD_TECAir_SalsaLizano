// =============================================================================
// File    : FlightClosingService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for flightclosing operations.
// =============================================================================

using TECAir.Core.DTOs.FlightClosing;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    public class FlightClosingService(
        IFlightRepository flightRepository,
        ICheckInRepository checkInRepository,
        IBaggageRepository baggageRepository,
        IReservationRepository reservationRepository,
        IUserRepository userRepository) : IFlightClosingService
    {
        private readonly IFlightRepository _flightRepository = flightRepository;
        private readonly ICheckInRepository _checkInRepository = checkInRepository;
        private readonly IBaggageRepository _baggageRepository = baggageRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IUserRepository _userRepository = userRepository;

        // ── Cierre ─────────────────────────────────────────────────────────────

        // Validates the flight, changes it to 'InAir', and returns the official passenger list.
        public async Task<FlightClosingDto> CloseFlightAsync(string flightNumber)
        {
            // Verificar que el vuelo exista antes de intentar cerrarlo
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            // Only flights in the Boarding state can be closed.
            if (flight.Status != FlightStatus.Boarding)
                throw new InvalidOperationException(
                    $"El vuelo '{flightNumber}' no puede cerrarse porque su estado actual es '{flight.Status}'. " +
                    $"Solo se pueden cerrar vuelos en estado 'Boarding'.");

            // Cambiar el estado del vuelo a InAir en la base de datos
            await _flightRepository.UpdateStatusAsync(flightNumber, FlightStatus.InAir);
            flight.Status = FlightStatus.InAir;

            // Construir y retornar la lista oficial con los pasajeros chequeados
            return await BuildFinalListAsync(flight);
        }

        // ── Consulta ───────────────────────────────────────────────────────────

        // Returns the official list without changing the state, for a pre-close review.
        public async Task<FlightClosingDto> GetFinalListAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            return await BuildFinalListAsync(flight);
        }

        // ── Private helper methods ──────────────────────────────────────────

        // Construye la lista oficial iterando sobre los check-ins del vuelo.
        // Each check-in is enriched with user data and baggage count.
        private async Task<FlightClosingDto> BuildFinalListAsync(Flight flight)
        {
            // Los check-ins representan la lista definitiva: solo quien hizo check-in viaja
            var checkIns = await _checkInRepository.GetByFlightNumberAsync(flight.FlightNumber);
            var passengers = new List<CheckedInPassengerDto>();

            foreach (var checkIn in checkIns)
            {
                // Retrieve the reservation to resolve the passenger user_id.
                var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
                var user = reservation is not null
                    ? await _userRepository.GetByIdAsync(reservation.UserId)
                    : null;

                // Count the baggage entries associated with this check-in reservation.
                var baggageCount = await _baggageRepository.CountByReservationCodeAsync(checkIn.ReservationCode);

                passengers.Add(new CheckedInPassengerDto
                {
                    CheckInId = checkIn.CheckInId,
                    Seat = checkIn.Seat,
                    BoardingGate = checkIn.BoardingGate,
                    FullName = user?.FullName ?? "Desconocido",
                    Email = user?.Email ?? "Sin correo",
                    BaggageCount = baggageCount,
                    BaggageSurcharge = CalcularCargoPorMaletas(baggageCount)
                });
            }

            return new FlightClosingDto
            {
                FlightNumber = flight.FlightNumber,
                Status = flight.Status.ToString(),
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                TotalPassengers = passengers.Count,
                TotalBaggages = passengers.Sum(p => p.BaggageCount),
                Passengers = passengers
            };
        }

        // Calculates the additional baggage charge according to issue #28:
        // 1st baggage → $0, 2nd → $50, 3rd and onward → $75 each.
        private static decimal CalcularCargoPorMaletas(int cantidad)
        {
            if (cantidad <= 1) return 0m;
            if (cantidad == 2) return 50m;

            // Starting with the 3rd baggage item, $75 is charged for each additional one.
            return 50m + ((cantidad - 2) * 75m);
        }
    }
}
