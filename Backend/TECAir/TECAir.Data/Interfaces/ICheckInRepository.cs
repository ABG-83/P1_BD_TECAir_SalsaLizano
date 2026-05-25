// =============================================================================
// Archivo  : ICheckInRepository.cs
// Capa     : TECAir.Data → Interfaces
// Propósito: Contrato de acceso a datos para la tabla 'check_ins'.
//
//            Métodos del Issue #15 (chequeo de pasajeros):
//              - CreateAsync              → registrar un nuevo check-in
//              - GetByIdAsync             → obtener un check-in por su ID
//              - GetByReservationIdAsync  → verificar si una reservación ya
//                                           tiene check-in (evitar duplicados)
//              - GetByFlightNumberAsync   → listar check-ins de un vuelo
//              - IsSeatTakenAsync         → verificar disponibilidad de asiento
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    // Contrato que deben cumplir todas las implementaciones del repositorio de check-ins
    public interface ICheckInRepository
    {
        // Inserta un nuevo registro de check-in en la base de datos.
        // Retorna el ID generado por la BD para el nuevo check-in.
        Task<int> CreateAsync(CheckIn checkIn);

        // Busca un check-in por su ID primario. Retorna null si no existe.
        Task<CheckIn?> GetByIdAsync(int checkInId);

        // Busca el check-in asociado a una reservación específica.
        // Retorna null si el pasajero aún no ha hecho check-in.
        // Se usa para detectar check-ins duplicados antes de crear uno nuevo.
        Task<CheckIn?> GetByReservationIdAsync(int reservationId);

        // Retorna todos los check-ins registrados para un vuelo.
        // Útil para listar los pasajeros ya chequeados en un vuelo.
        Task<IEnumerable<CheckIn>> GetByFlightNumberAsync(string flightNumber);

        // Verifica si un asiento ya está ocupado en un vuelo determinado.
        // Retorna true si el asiento ya fue asignado a otro pasajero.
        Task<bool> IsSeatTakenAsync(string flightNumber, string seat);
    }
}