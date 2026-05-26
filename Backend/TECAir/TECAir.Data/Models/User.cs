// =============================================================================
// File    : User.cs
// Layer   : TECAir.Data → Models
// Purpose : Represents a user account, its role, and profile metadata for TECAir.
// =============================================================================

namespace TECAir.Data.Models
{
    /// <summary>
    /// Defines the access levels available for TECAir user accounts.
    /// </summary>
    public enum UserRole
    {
        /// <summary>Administrative access for staff operations.</summary>
        ADMIN,

        /// <summary>Customer access for booking and travel workflows.</summary>
        CLIENT
    }

    /// <summary>
    /// Represents a user profile within the TECAir system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique database identifier for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the security role assigned to the user.
        /// </summary>
        public UserRole Role { get; set; } = UserRole.CLIENT;

        /// <summary>
        /// Gets or sets the total frequent flyer miles accumulated by the user.
        /// </summary>
        public float Miles { get; set; } = 0;

        /// <summary>
        /// Gets or sets the optional institutional identifier for students or staff.
        /// </summary>
        public string? CollegeIdNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional institutional or university affiliation.
        /// </summary>
        public string? College { get; set; } = string.Empty;
    }
}
