// =============================================================================
// File    : IBaggageService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for baggage operations.
// =============================================================================

using TECAir.Core.DTOs.Baggage;

namespace TECAir.Core.Interfaces
{
    public interface IBaggageService
    {
        // Validates passenger check-in, calculates the additional charge, and registers the baggage item.
        Task<BaggageResponseDto> AddBaggageAsync(BaggageDto dTO);

        // Returns all baggage items for a reservation with their charge breakdown.
        Task<IEnumerable<BaggageResponseDto>> GetByReservationCodeAsync(string reservationCode);

        // Finds a specific baggage item by its ID; returns null when it does not exist.
        Task<BaggageResponseDto?> GetByIdAsync(int baggageId);
    }
}
