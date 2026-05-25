// =============================================================================
// Archivo  : IAirplaneRepository.cs
// Capa     : TECAir.Data → Interfaces
// Propósito: Contrato de acceso a datos para la tabla AVION.
//            Se usa al registrar un vuelo para validar que el avión
//            asignado exista en la BD antes de hacer el INSERT.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    public interface IAirplaneRepository
    {
        // Retorna todos los aviones. Usado para el dropdown del formulario de vuelos.
        Task<IEnumerable<Airplane>> GetAllAsync();

        // Busca un avión por su matrícula. Retorna null si no existe.
        Task<Airplane?> GetByPlateNumberAsync(string plateNumber);
    }
}