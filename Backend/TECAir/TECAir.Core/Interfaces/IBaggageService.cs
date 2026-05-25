// =============================================================================
// Archivo  : IBaggageService.cs
// Capa     : TECAir.Core → Interfaces
// Propósito: Contrato de lógica de negocio para el Issue #16 — API de maletas.
// =============================================================================

using TECAir.Core.DTOs.Baggage;

namespace TECAir.Core.Interfaces
{
    public interface IBaggageService
    {
        // Valida el check-in del pasajero, calcula el cobro adicional y registra la maleta
        Task<BaggageResponseDto> AddBaggageAsync(BaggageDto dTO);

        // Retorna todas las maletas de una reservación con su desglose de cobros
        Task<IEnumerable<BaggageResponseDto>> GetByReservationIdAsync(int reservationId);

        // Busca una maleta específica por su ID; retorna null si no existe
        Task<BaggageResponseDto?> GetByIdAsync(int baggageId);
    }
}