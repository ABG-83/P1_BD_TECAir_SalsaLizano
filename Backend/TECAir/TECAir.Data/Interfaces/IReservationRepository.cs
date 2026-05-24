// =============================================================================
// Archivo  : IReservationRepository.cs
// Capa     : TECAir.Data → Interfaces
// Propósito: Contrato de acceso a datos para la entidad Reservation.
//            Para el Issue #29 (apertura de vuelos), el método principal es
//            GetPaidByFlightNumberAsync que retorna los pasajeros confirmados.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    // Contrato que deben cumplir todas las implementaciones del repositorio de reservaciones
    public interface IReservationRepository
    {
        // Retorna todas las reservaciones de un vuelo sin importar su estado de pago
        Task<IEnumerable<Reservation>> GetByFlightNumberAsync(string flightNumber);

        // Retorna solo las reservaciones con payment_status = 'paid' de un vuelo
        // Es el método central para la apertura de vuelos (pasajeros confirmados)
        Task<IEnumerable<Reservation>> GetPaidByFlightNumberAsync(string flightNumber);

        // Busca una reservación por su ID
        Task<Reservation?> GetByIdAsync(int reservationId);
    }
}