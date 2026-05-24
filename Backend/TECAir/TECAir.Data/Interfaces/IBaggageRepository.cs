// =============================================================================
// Archivo  : IBaggageRepository.cs
// Capa     : TECAir.Data → Interfaces
// Propósito: Contrato de acceso a datos para la entidad Baggage.
//            Para el Issue #29 se usa principalmente para contar las maletas
//            por reservación al generar el manifiesto de apertura de vuelo.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    // Contrato que deben cumplir todas las implementaciones del repositorio de maletas
    public interface IBaggageRepository
    {
        // Retorna todas las maletas asociadas a una reservación específica
        Task<IEnumerable<Baggage>> GetByReservationIdAsync(int reservationId);

        // Retorna el conteo total de maletas de una reservación
        // Usado en el manifiesto de apertura para mostrar la cantidad por pasajero
        Task<int> CountByReservationIdAsync(int reservationId);
    }
}