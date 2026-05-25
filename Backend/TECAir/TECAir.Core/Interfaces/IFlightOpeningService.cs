// =============================================================================
// Archivo  : IFlightOpeningService.cs
// Capa     : TECAir.Core → Interfaces
// Propósito: Contrato de lógica de negocio para la apertura de vuelos (Issue #29).
//            Separa la funcionalidad de apertura del servicio general de vuelos
//            porque involucra repositorios adicionales (reservaciones y maletas).
// =============================================================================

using TECAir.Core.DTOs.FlightOpening;

namespace TECAir.Core.Interfaces
{
    // Contrato para la operación de apertura de vuelos del aeropuerto
    public interface IFlightOpeningService
    {
        // Abre un vuelo cambiando su estado a 'Boarding' y retorna el manifiesto
        // de pasajeros confirmados con su conteo de maletas
        Task<FlightManifestDto> OpenFlightAsync(string flightNumber);

        // Retorna el manifiesto de pasajeros confirmados sin cambiar el estado del vuelo
        // Útil para consultar quiénes viajarán antes de confirmar la apertura
        Task<FlightManifestDto> GetManifestAsync(string flightNumber);
    }
}