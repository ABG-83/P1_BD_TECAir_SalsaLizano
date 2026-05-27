// =============================================================================
// File    : CheckInService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for checkin operations.
// =============================================================================

using TECAir.Core.DTOs.BoardingPass;
using TECAir.Core.DTOs.CheckIn;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    public class CheckInService(
        ICheckInRepository checkInRepository,
        IReservationRepository reservationRepository,
        IFlightRepository flightRepository,
        IUserRepository userRepository,
        IAirportRepository airportRepository) : ICheckInService
    {
        private readonly ICheckInRepository _checkInRepository = checkInRepository;
        private readonly IReservationRepository _reservationRepository = reservationRepository;
        private readonly IFlightRepository _flightRepository = flightRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IAirportRepository _airportRepository = airportRepository;

        // ── Check-in ───────────────────────────────────────────────────────────

        // Validates all business rules and registers the passenger check-in.
        public async Task<CheckInResponseDto> CheckInPassengerAsync(CheckInDto dto)
        {
            // Verify that the reservation exists.
            var reservation = await _reservationRepository.GetByCodeAsync(dto.ReservationCode)
                ?? throw new KeyNotFoundException(
                    $"No existe una reservación con ID {dto.ReservationCode}.");

            // Check-in is only allowed when the reservation was paid.
            if (reservation.PaymentState != PaymentStatus.Paid)
                throw new InvalidOperationException(
                    $"La reservación {dto.ReservationCode} no puede hacer check-in porque " +
                    $"su estado de pago es '{reservation.PaymentState}'. " +
                    "Solo las reservaciones 'paid' pueden proceder.");

            // Verificar que el vuelo exista
            var flight = await _flightRepository.GetByFlightNumberAsync(reservation.FlightNumber)
                ?? throw new KeyNotFoundException(
                    $"No existe el vuelo '{reservation.FlightNumber}' asociado a la reservación.");

            // Check-in is only possible while the flight is in the boarding process.
            if (flight.Status != FlightStatus.Boarding)
                throw new InvalidOperationException(
                    $"El vuelo '{flight.FlightNumber}' no está en fase de abordaje. " +
                    $"Estado actual: '{flight.Status}'. Solo se puede hacer check-in en vuelos 'Boarding'.");

            // Detect whether the passenger has already checked in for this reservation.
            var existingCheckIn = await _checkInRepository.GetByReservationCodeAsync(dto.ReservationCode);
            if (existingCheckIn is not null)
                throw new InvalidOperationException(
                    $"La reservación {dto.ReservationCode} ya tiene check-in registrado " +
                    $"(ID: {existingCheckIn.CheckInId}, Asiento: {existingCheckIn.Seat}).");

            // Normalize the seat to uppercase to avoid duplicates caused by case differences.
            var seatNormalized = dto.Seat.Trim().ToUpperInvariant();

            // Verify that the requested seat is free on the flight.
            var seatTaken = await _checkInRepository.IsSeatTakenAsync(flight.FlightNumber, seatNormalized);
            if (seatTaken)
                throw new InvalidOperationException(
                    $"El asiento '{seatNormalized}' ya está ocupado en el vuelo '{flight.FlightNumber}'. " +
                    "Por favor seleccione otro asiento.");

            // Construir el objeto de check-in con todos los datos validados
            var checkIn = new CheckIn
            {
                Seat = seatNormalized,
                BoardingGate = dto.BoardingGate.Trim().ToUpperInvariant(),
                PrintTime = DateTime.UtcNow,
                ReservationCode = reservation.ReservationCode,
                FlightNumber = flight.FlightNumber
            };

            // Persistir el check-in en la BD y obtener el ID asignado
            int newId = await _checkInRepository.CreateAsync(checkIn);
            checkIn.CheckInId = newId;

            // Retrieve the passenger name to include it in the confirmation response.
            var user = await _userRepository.GetByIdAsync(reservation.UserId);

            // Return the confirmation DTO with all relevant data.
            return MapToResponseDto(checkIn, user?.FullName ?? "Desconocido");
        }

        // ── Consultas ──────────────────────────────────────────────────────────

        // Finds a check-in by ID; returns null when it does not exist (the controller converts it into a 404 response).
        public async Task<CheckInResponseDto?> GetByIdAsync(int checkInId)
        {
            var checkIn = await _checkInRepository.GetByIdAsync(checkInId);
            if (checkIn is null) return null;

            // Resolve the passenger name through the chain: checkIn → reservation → user.
            var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
            var user = reservation is not null
                              ? await _userRepository.GetByIdAsync(reservation.UserId)
                              : null;

            return MapToResponseDto(checkIn, user?.FullName ?? "Desconocido");
        }

        // Genera el pase de abordar enriquecido con datos del vuelo y el pasajero
        public async Task<BoardingPassDto?> GetBoardingPassAsync(int checkInId)
        {
            // Find the base check-in.
            var checkIn = await _checkInRepository.GetByIdAsync(checkInId);
            if (checkIn is null) return null;

            // Resolver la cadena de datos: checkIn → reservation → user y flight → airports
            var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
            var flight = await _flightRepository.GetByFlightNumberAsync(checkIn.FlightNumber);

            // Resolver aeropuertos de origen y destino del vuelo para mostrarlos en el pase
            var originAirport = flight is not null
                                     ? await _airportRepository.GetByIdAsync(flight.OriginAirportId)
                                     : null;
            var destinationAirport = flight is not null
                                     ? await _airportRepository.GetByIdAsync(flight.DestinationAirportId)
                                     : null;

            // Resolve passenger details through the reservation.
            var user = reservation is not null
                       ? await _userRepository.GetByIdAsync(reservation.UserId)
                       : null;

            // Build the boarding pass with all fields required by the specification:
            // boarding gate, departure time, seat, and flight number.
            return new BoardingPassDto
            {
                CheckInId = checkIn.CheckInId,
                ReservationCode = checkIn.ReservationCode,
                FlightNumber = checkIn.FlightNumber,
                Seat = checkIn.Seat,
                BoardingGate = checkIn.BoardingGate,
                PrintTime = checkIn.PrintTime,
                DepartureTime = flight?.DepartureTime ?? DateTime.MinValue,
                ArrivalTime = flight?.ArrivalTime ?? DateTime.MinValue,
                OriginAirport = originAirport?.Name ?? "Desconocido",
                DestinationAirport = destinationAirport?.Name ?? "Desconocido",
                PassengerName = user?.FullName ?? "Desconocido",
                PassengerEmail = user?.Email ?? "Sin correo"
            };
        }

        // Returns the existing check-in for a reservation code, or null if not yet checked in.
        public async Task<CheckInResponseDto?> GetByReservationCodeAsync(string reservationCode)
        {
            var checkIn = await _checkInRepository.GetByReservationCodeAsync(reservationCode);
            if (checkIn is null) return null;

            var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
            var user = reservation is not null
                       ? await _userRepository.GetByIdAsync(reservation.UserId)
                       : null;

            return MapToResponseDto(checkIn, user?.FullName ?? "Desconocido");
        }

        // Returns all check-ins for a flight so the staff member can review the boarding status.
        public async Task<IEnumerable<CheckInResponseDto>> GetByFlightNumberAsync(string flightNumber)
        {
            var checkIns = await _checkInRepository.GetByFlightNumberAsync(flightNumber);
            var result = new List<CheckInResponseDto>();

            // Para cada check-in, resolver el nombre del pasajero de forma individual
            foreach (var checkIn in checkIns)
            {
                var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
                var user = reservation is not null
                                  ? await _userRepository.GetByIdAsync(reservation.UserId)
                                  : null;

                result.Add(MapToResponseDto(checkIn, user?.FullName ?? "Desconocido"));
            }

            return result;
        }

        // ── Private helper method ────────────────────────────────────────────

        // Converts a CheckIn model and the passenger name into a CheckInResponseDto.
        private static CheckInResponseDto MapToResponseDto(CheckIn checkIn, string passengerName) => new()
        {
            CheckInId = checkIn.CheckInId,
            Seat = checkIn.Seat,
            BoardingGate = checkIn.BoardingGate,
            PrintTime = checkIn.PrintTime,
            ReservationCode = checkIn.ReservationCode,
            FlightNumber = checkIn.FlightNumber,
            PassengerName = passengerName
        };
    }
}
