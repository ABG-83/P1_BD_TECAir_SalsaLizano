// =============================================================================
// Archivo  : IPromotionRepository.cs
// Capa     : TECAir.Data → Interfaces
// Propósito: Contrato de acceso a datos para la entidad Promotion.
//            Define las operaciones CRUD que PromotionRepository implementa
//            usando ADO.NET puro contra la tabla 'promotions'.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    // Contrato que deben cumplir todas las implementaciones del repositorio de promociones
    public interface IPromotionRepository
    {
        // Obtiene todas las promociones del sistema ordenadas por fecha de inicio descendente
        Task<IEnumerable<Promotion>> GetAllAsync();

        // Obtiene solo las promociones marcadas como activas para mostrar a los clientes
        Task<IEnumerable<Promotion>> GetActiveAsync();

        // Busca una promoción específica por su identificador único
        Task<Promotion?> GetByIdAsync(int promotionId);

        // Inserta una nueva promoción y retorna el ID auto-generado
        Task<int> CreateAsync(Promotion promotion);

        // Actualiza todos los campos editables de una promoción existente
        Task UpdateAsync(Promotion promotion);

        // Elimina permanentemente una promoción de la base de datos
        Task DeleteAsync(int promotionId);
    }
}