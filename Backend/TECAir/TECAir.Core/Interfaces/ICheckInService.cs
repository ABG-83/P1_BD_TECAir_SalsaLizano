// =============================================================================
// Archivo  : ICheckInService.cs
// Capa     : TECAir.Core → Interfaces
// Propósito: Contrato de la lógica de negocio para el chequeo de pasajeros.
//
//            Scope del Issue #15 (chequeo de pasajeros):
//              - Realizar el check-in de un pasajero (validar reservación,
//                asignar asiento y registrar el pase de abordar)
//              - Consultar un check-in por su ID
//              - Obtener el pase de abordar completo de un check-in
//              - Listar los check-ins de un vuelo
// =============================================================================

using TECAir.Core.DTOs.BoardingPass;
using TECAir.Core.DTOs.CheckIn;

namespace TECAir.Core.Interfaces
{
    // Contrato que define las operaciones de negocio para el check-in de pasajeros
    public interface ICheckInService
    {
        // Realiza el check-in de un pasajero dado su reservación, asiento y puerta.
        // Lanza InvalidOperationException si:
        //   - La reservación no existe o no está pagada
        //   - El vuelo no está en estado 'Boarding'
        //   - El pasajero ya tiene check-in en esa reservación
        //   - El asiento ya está ocupado en ese vuelo
        // Retorna el DTO de confirmación con los datos del check-in creado.
        Task<CheckInResponseDto> CheckInPassengerAsync(CheckInDto dto);

        // Busca un check-in por su ID primario.
        // Retorna null si no existe — el controlador lo convierte en 404.
        Task<CheckInResponseDto?> GetByIdAsync(int checkInId);

        // Genera el pase de abordar completo para un check-in existente.
        // Enriquece los datos del check-in con información del vuelo y el pasajero.
        // Retorna null si el check-in no existe.
        Task<BoardingPassDto?> GetBoardingPassAsync(int checkInId);

        // Retorna todos los check-ins registrados para un vuelo determinado.
        // Útil para que el funcionario vea quiénes ya han hecho check-in.
        Task<IEnumerable<CheckInResponseDto>> GetByFlightNumberAsync(string flightNumber);
    }
}