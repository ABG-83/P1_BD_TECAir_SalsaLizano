using TECAir.Core.DTOs.Users;

namespace TECAir.Core.Interfaces
{
    /// <summary>
    /// Defines business-logic operations for user management.
    /// </summary>
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);

        /// <summary>Creates a new user and returns the generated ID.</summary>
        Task<int> CreateUserAsync(UserRequestDto dto);

        /// <summary>Updates profile fields; returns false if the user does not exist.</summary>
        Task<bool> UpdateUserAsync(int userId, UserRequestDto dto);

        /// <summary>Deletes a user; returns false if the user does not exist.</summary>
        Task<bool> DeleteUserAsync(int userId);
    }
}
