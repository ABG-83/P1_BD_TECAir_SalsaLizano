// =============================================================================
// File    : IReservationService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for reservation operations.
// =============================================================================

using TECAir.Data.Models;
using TECAir.Core.DTOs.Reservations;

namespace TECAir.Core.Interfaces
{
    /// <summary>
    /// Defines business logic orchestration operations for flight reservations.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Gets all reservations in the system.
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync();

        /// <summary>
        /// Searches reservations by partial user full name.
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> SearchReservationsByNameAsync(string name);

        /// <summary>
        /// Creates a new flight reservation ledger record after executing safety assertions.
        /// </summary>
        Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto);

        /// <summary>
        /// Consults a single reservation record matching its unique alphanumeric reference code.
        /// </summary>
        Task<ReservationResponseDto?> GetReservationByCodeAsync(string reservationCode);

        /// <summary>
        /// Consults the complete collection of reservation itineraries registered under a specific user account ID.
        /// </summary>
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByUserIdAsync(int userId);

        /// <summary>
        /// Modifies an existing reservation's payment status, timestamps, or structural metadata safely.
        /// </summary>
        Task<bool> ModifyReservationAsync(string reservationCode, CreateReservationDto dto);

        /// <summary>
        /// Cancels a reservation by executing specific business rules and moving its workflow status.
        /// </summary>
        Task<bool> CancelReservationAsync(string reservationCode);
    }
}
