// =============================================================================
// Archivo  : FlightClosingService.cs
// Capa     : TECAir.Core → Services
// Propósito: Implementa la lógica de negocio para el cierre de vuelos.
//            Coordina cuatro repositorios: vuelos, check-ins, maletas y usuarios.
//
//            Diferencia clave con la apertura (Issue #29):
//              - Apertura usa reservaciones 'paid' → pasajeros que podrían viajar
//              - Cierre usa check-ins              → pasajeros que definitivamente viajarán
//
//            Reglas de negocio aplicadas:
//              - Solo se pueden cerrar vuelos en estado 'Boarding'
//              - La lista oficial solo incluye pasajeros con check-in registrado
//              - El cargo por maletas sigue la misma regla del Issue #28
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

        // Valida el vuelo, lo cambia a 'InAir' y retorna la lista oficial de pasajeros
        public async Task<FlightClosingDto> CloseFlightAsync(string flightNumber)
        {
            // Verificar que el vuelo exista antes de intentar cerrarlo
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            // Solo se pueden cerrar vuelos que estén en estado Boarding
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

        // Retorna la lista oficial sin cambiar el estado, para consulta previa al cierre
        public async Task<FlightClosingDto> GetFinalListAsync(string flightNumber)
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(flightNumber)
                ?? throw new KeyNotFoundException($"No existe el vuelo '{flightNumber}'.");

            return await BuildFinalListAsync(flight);
        }

        // ── Métodos privados de apoyo ──────────────────────────────────────────

        // Construye la lista oficial iterando sobre los check-ins del vuelo.
        // Cada check-in se enriquece con datos del usuario y conteo de maletas.
        private async Task<FlightClosingDto> BuildFinalListAsync(Flight flight)
        {
            // Los check-ins representan la lista definitiva: solo quien hizo check-in viaja
            var checkIns = await _checkInRepository.GetByFlightNumberAsync(flight.FlightNumber);
            var passengers = new List<CheckedInPassengerDto>();

            foreach (var checkIn in checkIns)
            {
                // Obtener la reservación para llegar al user_id del pasajero
                var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
                var user = reservation is not null
                    ? await _userRepository.GetByIdAsync(reservation.UserId)
                    : null;

                // Contar las maletas de la reservación asociada a este check-in
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

        // Calcula el cargo adicional por maletas según la regla del Issue #28:
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