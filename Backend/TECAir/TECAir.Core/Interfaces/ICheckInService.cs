// =============================================================================
// File    : ICheckInService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for checkn operations.
// =============================================================================

using TECAir.Core.DTOs.BoardingPass;
using TECAir.Core.DTOs.CheckIn;

namespace TECAir.Core.Interfaces
{
    // Contract that defines the business operations for passenger check-in.
    public interface ICheckInService
    {
        // Performs passenger check-in for the provided reservation, seat, and boarding gate.
        // Lanza InvalidOperationException si:
        //   - The reservation does not exist or is not paid.
        //   - The flight is not in the 'Boarding' state.
        //   - The passenger already has a check-in for this reservation.
        //   - The seat is already occupied on that flight.
        // Returns the confirmation DTO with the created check-in data.
        Task<CheckInResponseDto> CheckInPassengerAsync(CheckInDto dto);

        // Finds a check-in by its primary ID.
        // Returns null when it does not exist; the controller converts it into a 404 response.
        Task<CheckInResponseDto?> GetByIdAsync(int checkInId);

        // Genera el pase de abordar completo para un check-in existente.
        // Enriches the check-in data with flight and passenger information.
        // Returns null when the check-in does not exist.
        Task<BoardingPassDto?> GetBoardingPassAsync(int checkInId);

        // Returns all check-ins recorded for a specific flight.
        // Useful for the staff member to see who has already checked in.
        Task<IEnumerable<CheckInResponseDto>> GetByFlightNumberAsync(string flightNumber);

        // Finds an existing check-in by reservation code. Returns null if none exists.
        Task<CheckInResponseDto?> GetByReservationCodeAsync(string reservationCode);
    }
}
