// =============================================================================
// File    : IFlightClosingService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for flightclosing operations.
// =============================================================================

using TECAir.Core.DTOs.FlightClosing;

namespace TECAir.Core.Interfaces
{
    // Contract for the airport flight-closing operation.
    public interface IFlightClosingService
    {
        // Closes a flight by changing its status to 'InAir' and returns the official
        // list for passengers who have confirmed check-in with their baggage count.
        Task<FlightClosingDto> CloseFlightAsync(string flightNumber);

        // Returns the official passenger list with check-in without changing the flight status.
        // Useful for previewing the final list before confirming the closure.
        Task<FlightClosingDto> GetFinalListAsync(string flightNumber);
    }
}
