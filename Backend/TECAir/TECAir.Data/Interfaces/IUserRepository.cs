// =============================================================================
// File    : IUserRepository.cs
// Layer   : TECAir.Data → Interfaces
// Purpose : Defines the data access contract for user records.
// =============================================================================

using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data access operations for user records.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets all users registered in the system.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Gets a user by its identifier, or returns null when no match exists.
        /// </summary>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>
        /// Gets a user by email, or returns null when no match exists.
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Creates a new user record and returns the generated identifier.
        /// </summary>
        Task<int> CreateAsync(User user);

        /// <summary>
        /// Updates an existing user record.
        /// </summary>
        Task<bool> UpdateAsync(User user);

        /// <summary>
        /// Deletes a user record by its identifier.
        /// </summary>
        Task<bool> DeleteAsync(int userId);
    }
}
