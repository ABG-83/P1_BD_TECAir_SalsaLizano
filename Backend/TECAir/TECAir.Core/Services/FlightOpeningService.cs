// =============================================================================
// Archivo  : FlightOpeningService.cs
// Capa     : TECAir.Core → Services
// Propósito: Implementa la lógica de negocio para la apertura de vuelos.
//            Coordina tres repositorios: vuelos, reservaciones y maletas.
//
//            Reglas de negocio aplicadas:
//              - Solo se pueden abrir vuelos en estado 'Scheduled'
//              - Solo los pasajeros con reservación 'paid' aparecen en el manifiesto
//              - El cargo por maletas sigue la regla: 1ra gratis, 2da $50, 3ra+ $75 c/u
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
        private readonly IFlightRepository      _flightRepository      = flightRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IBaggageRepository     _baggageRepository     = baggageRepository;
        private readonly IUserRepository        _userRepository        = userRepository;

        // ── Apertura ───────────────────────────────────────────────────────────

        // Valida el vuelo, lo cambia a 'Boarding' y retorna el manifiesto de pasajeros
        public async Task<FlightManifestDto> OpenFlightAsync(string flightNumber)
        {
            // Verificar que el vuelo exista antes de intentar abrirlo
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            // Solo se pueden abrir vuelos que estén en estado Scheduled
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

        // Retorna el manifiesto sin cambiar el estado, para consulta previa a la apertura
        public async Task<FlightManifestDto> GetManifestAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            return await BuildManifestAsync(flight);
        }

        // ── Métodos privados de apoyo ──────────────────────────────────────────

        // Construye el manifiesto iterando sobre las reservaciones pagadas del vuelo
        // y consultando las maletas de cada una para armar el detalle por pasajero
        private async Task<FlightManifestDto> BuildManifestAsync(Flight flight)
        {
            var paidReservations = await _reservationRepository.GetPaidByFlightNumberAsync(flight.FlightNumber);
            var passengers = new List<PassengerManifestItemDto>();

            foreach (var reservation in paidReservations)
            {
                // Obtener datos del pasajero para mostrar nombre y correo en el manifiesto
                var user = await _userRepository.GetByIdAsync(reservation.UserId);

                // Contar las maletas registradas para esta reservación
                var baggageCount = await _baggageRepository.CountByReservationIdAsync(reservation.ReservationId);

                passengers.Add(new PassengerManifestItemDto
                {
                    ReservationId   = reservation.ReservationId,
                    FullName        = user?.FullName ?? "Desconocido",
                    Email           = user?.Email   ?? "Sin correo",
                    BaggageCount    = baggageCount,
                    BaggageSurcharge = CalcularCargoPorMaletas(baggageCount)
                });
            }

            return new FlightManifestDto
            {
                FlightNumber    = flight.FlightNumber,
                Status          = flight.Status.ToString(),
                DepartureTime   = flight.DepartureTime,
                ArrivalTime     = flight.ArrivalTime,
                TotalPassengers = passengers.Count,
                TotalBaggages   = passengers.Sum(p => p.BaggageCount),
                Passengers      = passengers
            };
        }

        // Calcula el cargo adicional por maletas según la regla de negocio:
        // 1ra maleta → $0, 2da → $50, 3ra en adelante → $75 cada una
        private static decimal CalcularCargoPorMaletas(int cantidad)
        {
            if (cantidad <= 1) return 0m;
            if (cantidad == 2) return 50m;

            // Desde la 3ra maleta se cobran $75 por cada una adicional
            return 50m + ((cantidad - 2) * 75m);
        }
    }
}