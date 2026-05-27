// =============================================================================
// File    : UserRequestDto.cs
// Layer   : TECAir.Core → DTOs
// Purpose : Defines request and response payloads used by the application.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TECAir.Core.DTOs.Users
{
    /// <summary>
    /// Payload for creating or updating a new user account.
    /// </summary>
    public class UserRequestDto
    {
        [Required]
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string CollegeIdNumber { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? Password { get; set; }
    }
}
