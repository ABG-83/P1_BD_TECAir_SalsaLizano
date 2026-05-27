// =============================================================================
// File    : TokenResponseDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

namespace TECAir.Core.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public float Miles { get; set; }
    }
}
