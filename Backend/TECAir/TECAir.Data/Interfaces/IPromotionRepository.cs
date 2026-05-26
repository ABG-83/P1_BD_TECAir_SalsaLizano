// =============================================================================
// File    : IPromotionRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for promotion records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for promotion records.
    /// </summary>
    public interface IPromotionRepository
    {
        /// <summary>
        /// Gets all promotions registered in the system.
        /// </summary>
        Task<IEnumerable<Promotion>> GetAllAsync();

        /// <summary>
        /// Gets only the active promotions.
        /// </summary>
        Task<IEnumerable<Promotion>> GetActiveAsync();

        /// <summary>
        /// Gets a promotion by its identifier, or returns null when no match exists.
        /// </summary>
        Task<Promotion?> GetByIdAsync(int promotionId);

        /// <summary>
        /// Creates a new promotion record and returns the generated identifier.
        /// </summary>
        Task<int> CreateAsync(Promotion promotion);

        /// <summary>
        /// Updates an existing promotion record.
        /// </summary>
        Task UpdateAsync(Promotion promotion);

        /// <summary>
        /// Deletes a promotion record by its identifier.
        /// </summary>
        Task DeleteAsync(int promotionId);
    }
}
