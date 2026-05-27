// =============================================================================
// File    : IAuthService.cs
// Layer   : TECAir.Core → Interfaces
// Purpose : Defines contracts for auth operations.
// =============================================================================

using TECAir.Core.DTOs.Auth;

namespace TECAir.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
