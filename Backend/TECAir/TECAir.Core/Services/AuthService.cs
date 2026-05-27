// =============================================================================
// File    : AuthService.cs
// Layer   : TECAir.Core → Services
// Purpose : Contains business logic for auth operations.
// =============================================================================

using TECAir.Core.DTOs.Auth;
using TECAir.Core.Interfaces;
using TECAir.Data.Interfaces;

namespace TECAir.Core.Services
{
    public class AuthService(IUserRepository userRepository) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            return new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Miles = user.Miles
            };
        }
    }
}
