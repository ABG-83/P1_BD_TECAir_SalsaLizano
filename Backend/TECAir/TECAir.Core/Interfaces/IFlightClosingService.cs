// =============================================================================
// Archivo  : IFlightClosingService.cs
// Capa     : TECAir.Core → Interfaces
// Propósito: Contrato de lógica de negocio para el cierre de vuelos (Issue #30).
//            Separa la funcionalidad de cierre del servicio general de vuelos
//            porque genera la lista oficial basada en check-ins, no en reservaciones.
// =============================================================================

using TECAir.Core.DTOs.FlightClosing;

namespace TECAir.Core.Interfaces
{
    // Contrato para la operación de cierre de vuelos del aeropuerto
    public interface IFlightClosingService
    {
        // Cierra un vuelo cambiando su estado a 'InAir' y retorna la lista oficial
        // de pasajeros que tienen check-in confirmado con su conteo de maletas
        Task<FlightClosingDto> CloseFlightAsync(string flightNumber);

        // Retorna la lista oficial de pasajeros con check-in sin cambiar el estado del vuelo
        // Útil para previsualizar la lista final antes de confirmar el cierre
        Task<FlightClosingDto> GetFinalListAsync(string flightNumber);
    }
}