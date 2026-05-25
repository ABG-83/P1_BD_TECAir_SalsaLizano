// =============================================================================
// Archivo  : CheckInService.cs
// Capa     : TECAir.Core → Services
// Propósito: Implementa la lógica de negocio para el chequeo de pasajeros.
//            Coordina cuatro repositorios: check-ins, reservaciones,
//            vuelos y usuarios.
//
//            Reglas de negocio aplicadas:
//              - Solo se puede hacer check-in en vuelos con estado 'Boarding'
//              - La reservación debe existir y tener payment_status = 'paid'
//              - Un pasajero no puede hacer check-in dos veces en la misma reservación
//              - El asiento debe estar disponible (no asignado a otro pasajero)
//              - El asiento se almacena en mayúsculas para evitar duplicados por case
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

        // Valida todas las reglas de negocio y registra el check-in del pasajero
        public async Task<CheckInResponseDto> CheckInPassengerAsync(CheckInDto dto)
        {
            // Verificar que la reservación exista
            var reservation = await _reservationRepository.GetByCodeAsync(dto.ReservationCode)
                ?? throw new KeyNotFoundException(
                    $"No existe una reservación con ID {dto.ReservationCode}.");

            // Solo se puede hacer check-in si la reservación fue pagada
            if (reservation.PaymentState != PaymentStatus.Paid)
                throw new InvalidOperationException(
                    $"La reservación {dto.ReservationCode} no puede hacer check-in porque " +
                    $"su estado de pago es '{reservation.PaymentState}'. " +
                    "Solo las reservaciones 'paid' pueden proceder.");

            // Verificar que el vuelo exista
            var flight = await _flightRepository.GetByFlightNumberAsync(reservation.FlightNumber)
                ?? throw new KeyNotFoundException(
                    $"No existe el vuelo '{reservation.FlightNumber}' asociado a la reservación.");

            // El check-in solo es posible si el vuelo está en proceso de abordaje
            if (flight.Status != FlightStatus.Boarding)
                throw new InvalidOperationException(
                    $"El vuelo '{flight.FlightNumber}' no está en fase de abordaje. " +
                    $"Estado actual: '{flight.Status}'. Solo se puede hacer check-in en vuelos 'Boarding'.");

            // Detectar si el pasajero ya realizó check-in para esta reservación
            var existingCheckIn = await _checkInRepository.GetByReservationCodeAsync(dto.ReservationCode);
            if (existingCheckIn is not null)
                throw new InvalidOperationException(
                    $"La reservación {dto.ReservationCode} ya tiene check-in registrado " +
                    $"(ID: {existingCheckIn.CheckInId}, Asiento: {existingCheckIn.Seat}).");

            // Normalizar el asiento a mayúsculas para evitar duplicados por diferencia de case
            var seatNormalized = dto.Seat.Trim().ToUpperInvariant();

            // Verificar que el asiento solicitado esté libre en el vuelo
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

            // Obtener el nombre del pasajero para incluirlo en la respuesta de confirmación
            var user = await _userRepository.GetByIdAsync(reservation.UserId);

            // Retornar el DTO de confirmación con todos los datos relevantes
            return MapToResponseDto(checkIn, user?.FullName ?? "Desconocido");
        }

        // ── Consultas ──────────────────────────────────────────────────────────

        // Busca un check-in por ID; retorna null si no existe (el controlador lo convierte en 404)
        public async Task<CheckInResponseDto?> GetByIdAsync(int checkInId)
        {
            var checkIn = await _checkInRepository.GetByIdAsync(checkInId);
            if (checkIn is null) return null;

            // Resolver el nombre del pasajero a través de la cadena: checkIn → reservation → user
            var reservation = await _reservationRepository.GetByCodeAsync(checkIn.ReservationCode);
            var user = reservation is not null
                              ? await _userRepository.GetByIdAsync(reservation.UserId)
                              : null;

            return MapToResponseDto(checkIn, user?.FullName ?? "Desconocido");
        }

        // Genera el pase de abordar enriquecido con datos del vuelo y el pasajero
        public async Task<BoardingPassDto?> GetBoardingPassAsync(int checkInId)
        {
            // Buscar el check-in base
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

            // Resolver datos del pasajero a través de la reservación
            var user = reservation is not null
                       ? await _userRepository.GetByIdAsync(reservation.UserId)
                       : null;

            // Construir el pase de abordar con todos los campos requeridos por la especificación:
            // puerta de abordaje, hora de salida, asiento y número de vuelo
            return new BoardingPassDto
            {
                CheckInId = checkIn.CheckInId,
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

        // Retorna todos los check-ins de un vuelo para que el funcionario vea el estado de abordaje
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

        // ── Método privado de apoyo ────────────────────────────────────────────

        // Convierte un modelo CheckIn y el nombre del pasajero en un CheckInResponseDto
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
