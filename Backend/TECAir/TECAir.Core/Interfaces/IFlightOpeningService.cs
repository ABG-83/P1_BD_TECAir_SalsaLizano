// =============================================================================
// File    : IFlightOpeningService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for flightopening operations.
// =============================================================================

using TECAir.Core.DTOs.FlightOpening;

namespace TECAir.Core.Interfaces
{
    // Contract for the airport flight-opening operation.
    public interface IFlightOpeningService
    {
        // Abre un vuelo cambiando su estado a 'Boarding' y retorna el manifiesto
        // for confirmed passengers with their baggage count.
        Task<FlightManifestDto> OpenFlightAsync(string flightNumber);

        // Returns the confirmed passenger manifest without changing the flight status.
        // Useful for reviewing who will travel before confirming the opening.
        Task<FlightManifestDto> GetManifestAsync(string flightNumber);
    }
}
