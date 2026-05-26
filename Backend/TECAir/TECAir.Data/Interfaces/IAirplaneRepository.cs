// =============================================================================
// File    : IAirplaneRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for airplane records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for airplane records.
    /// </summary>
    public interface IAirplaneRepository
    {
        /// <summary>
        /// Gets all airplanes currently registered in the system.
        /// </summary>
        Task<IEnumerable<Airplane>> GetAllAsync();

        /// <summary>
        /// Gets an airplane by its plate number, or returns null when no match exists.
        /// </summary>
        Task<Airplane?> GetByPlateNumberAsync(string plateNumber);
    }
}
