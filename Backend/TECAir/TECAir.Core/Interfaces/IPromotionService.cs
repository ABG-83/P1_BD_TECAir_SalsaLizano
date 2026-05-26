// =============================================================================
// File    : IPromotionService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for promotion operations.
// =============================================================================

using TECAir.Core.DTOs.Promotions;

namespace TECAir.Core.Interfaces
{
    // Contract that every promotion service implementation must satisfy.
    public interface IPromotionService
    {
        // Gets all promotions, active and inactive, for the admin view.
        Task<IEnumerable<PromotionResponseDto>> GetAllPromotionsAsync();

        // Gets only the active promotions for the client promotions area.
        Task<IEnumerable<PromotionResponseDto>> GetActivePromotionsAsync();

        // Finds a promotion by its ID and returns null when it does not exist.
        Task<PromotionResponseDto?> GetPromotionByIdAsync(int promotionId);

        // Validates the business rules and registers a new promotion, returning the generated ID.
        Task<int> CreatePromotionAsync(CreatePromotionDto dto);

        // Validates and updates all fields of an existing promotion.
        Task UpdatePromotionAsync(int promotionId, UpdatePromotionDto dto);

        // Deletes a promotion permanently by its ID.
        Task DeletePromotionAsync(int promotionId);
    }
}
