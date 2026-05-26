// =============================================================================
// File    : UserService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for user operations.
// =============================================================================

using TECAir.Core.DTOs.Users;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;
using TECAir.Data.Models;

namespace TECAir.Core.Services
{
    /// <summary>
    /// Implements business logic for user management, delegating persistence
    /// to <see cref="IUserRepository"/>.
    /// </summary>
    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Projects a <see cref="User"/> model to a <see cref="UserResponseDto"/>.</summary>
        private static UserResponseDto ToDto(User user) => new()
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            Miles = user.Miles,
            CollegeIdNumber = user.CollegeIdNumber,
            College = user.College
        };

        // ── Read ───────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(ToDto);
        }

        /// <inheritdoc/>
        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user is null ? null : ToDto(user);
        }

        /// <inheritdoc/>
        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user is null ? null : ToDto(user);
        }

        // ── Write ──────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<int> CreateUserAsync(UserRequestDto dto)
        {
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.CLIENT,
                CollegeIdNumber = dto.CollegeIdNumber,
                College = dto.College
            };

            return await _userRepository.CreateAsync(user);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUserAsync(int userId, UserRequestDto dto)
        {
            var existing = await _userRepository.GetByIdAsync(userId);
            if (existing is null) return false;

            // Apply only the fields provided in the request
            existing.FullName = dto.FullName ?? existing.FullName;
            existing.Email = dto.Email ?? existing.Email;
            existing.PhoneNumber = dto.PhoneNumber ?? existing.PhoneNumber;
            existing.CollegeIdNumber = dto.CollegeIdNumber ?? existing.CollegeIdNumber;
            existing.College = dto.College ?? existing.College;

            return await _userRepository.UpdateAsync(existing);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteUserAsync(int userId) =>
            await _userRepository.DeleteAsync(userId);
    }
}
