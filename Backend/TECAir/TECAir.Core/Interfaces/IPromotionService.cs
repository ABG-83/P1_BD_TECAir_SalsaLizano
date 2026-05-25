// =============================================================================
// Archivo  : IPromotionService.cs
// Capa     : TECAir.Core → Interfaces
// Propósito: Contrato de lógica de negocio para la gestión de promociones.
//            Separa la capa de servicios del controlador y permite mockear
//            en pruebas unitarias sin depender de la base de datos.
// =============================================================================

using TECAir.Core.DTOs.Promotions;

namespace TECAir.Core.Interfaces
{
    // Contrato que deben cumplir todas las implementaciones del servicio de promociones
    public interface IPromotionService
    {
        // Obtiene todas las promociones (activas e inactivas) para la vista de administrador
        Task<IEnumerable<PromotionResponseDto>> GetAllPromotionsAsync();

        // Obtiene solo las promociones activas para el área de promociones de la vista cliente
        Task<IEnumerable<PromotionResponseDto>> GetActivePromotionsAsync();

        // Busca una promoción por su ID, retorna null si no existe
        Task<PromotionResponseDto?> GetPromotionByIdAsync(int promotionId);

        // Valida las reglas de negocio y registra una nueva promoción, retorna el ID generado
        Task<int> CreatePromotionAsync(CreatePromotionDto dto);

        // Valida y actualiza todos los campos de una promoción existente
        Task UpdatePromotionAsync(int promotionId, UpdatePromotionDto dto);

        // Elimina permanentemente una promoción por su ID
        Task DeletePromotionAsync(int promotionId);
    }
}