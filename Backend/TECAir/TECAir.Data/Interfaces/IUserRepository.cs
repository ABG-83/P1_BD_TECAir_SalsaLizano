using TECAir.Data.Models;

namespace TECAir.Data.Interfaces
{
    /// <summary>
    /// Defines data-access operations for <see cref="User"/> records.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Returns all users in the system.</summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>Returns a single user by primary key, or null if not found.</summary>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>Returns a single user by email, or null if not found.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Inserts a new user and returns the generated ID.</summary>
        Task<int> CreateAsync(User user);

        /// <summary>Updates mutable profile fields of an existing user.</summary>
        Task<bool> UpdateAsync(User user);

        /// <summary>Permanently removes a user record.</summary>
        Task<bool> DeleteAsync(int userId);
    }
}
