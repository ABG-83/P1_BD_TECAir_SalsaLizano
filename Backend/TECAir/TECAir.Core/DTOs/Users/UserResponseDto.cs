namespace TECAir.Core.DTOs.Users
{
    /// <summary>
    /// Represents a user record returned to the client.
    /// </summary>
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public float Miles { get; set; }
        public string? CollegeIdNumber { get; set; } = string.Empty;
        public string? College { get; set; } = string.Empty;
    }
}
