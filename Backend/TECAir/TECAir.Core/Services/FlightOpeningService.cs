// =============================================================================
// File    : FlightOpeningService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for flightopening operations.
// =============================================================================

using TECAir.Core.DTOs.FlightOpening;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    public class FlightOpeningService(
        IFlightRepository flightRepository,
        IReservationRepository reservationRepository,
        IBaggageRepository baggageRepository,
        IUserRepository userRepository) : IFlightOpeningService
    {
        private readonly IFlightRepository _flightRepository = flightRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IBaggageRepository _baggageRepository = baggageRepository;
        private readonly IUserRepository _userRepository = userRepository;

        // ── Apertura ───────────────────────────────────────────────────────────

        // Validates the flight, changes it to 'Boarding', and returns the passenger manifest.
        public async Task<FlightManifestDto> OpenFlightAsync(string flightNumber)
        {
            // Verificar que el vuelo exista antes de intentar abrirlo
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            // Only flights in the Scheduled state can be opened.
            if (flight.Status != FlightStatus.Scheduled)
                throw new InvalidOperationException(
                    $"El vuelo '{flightNumber}' no puede abrirse porque su estado actual es '{flight.Status}'. " +
                    $"Solo se pueden abrir vuelos en estado 'Scheduled'.");

            // Cambiar el estado del vuelo a Boarding en la base de datos
            await _flightRepository.UpdateStatusAsync(flightNumber, FlightStatus.Boarding);
            flight.Status = FlightStatus.Boarding;

            // Construir y retornar el manifiesto con los pasajeros confirmados
            return await BuildManifestAsync(flight);
        }

        // ── Consulta ───────────────────────────────────────────────────────────

        // Returns the manifest without changing the state, for a pre-opening review.
        public async Task<FlightManifestDto> GetManifestAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            return await BuildManifestAsync(flight);
        }

        // ── Private helper methods ──────────────────────────────────────────

        // Construye el manifiesto iterando sobre las reservaciones pagadas del vuelo
        // and checking each baggage item to build the passenger detail.
        private async Task<FlightManifestDto> BuildManifestAsync(Flight flight)
        {
            var paidReservations = await _reservationRepository.GetPaidByFlightNumberAsync(flight.FlightNumber);
            var passengers = new List<PassengerManifestItemDto>();

            foreach (var reservation in paidReservations)
            {
                // Obtener datos del pasajero para mostrar nombre y correo en el manifiesto
                var user = await _userRepository.GetByIdAsync(reservation.UserId);

                // Count the baggage items registered for this reservation.
                var baggageCount = await _baggageRepository.CountByReservationCodeAsync(reservation.ReservationCode);

                passengers.Add(new PassengerManifestItemDto
                {
                    ReservationCode = reservation.ReservationCode,
                    FullName = user?.FullName ?? "Desconocido",
                    Email = user?.Email ?? "Sin correo",
                    BaggageCount = baggageCount,
                    BaggageSurcharge = CalcularCargoPorMaletas(baggageCount)
                });
            }

            return new FlightManifestDto
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

        // Calculates the additional baggage charge according to the business rule:
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
