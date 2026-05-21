namespace TECAir.Data.Models
{
    /// <summary>
    /// Defines system access levels for user accounts.
    /// </summary>
    public enum UserRole
    {
        ADMIN,
        CLIENT
    }

    /// <summary>
    /// Represents a user profile within the TECAir system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier and primary key for the database record.
        /// </summary>
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.CLIENT;

        /// <summary>
        /// Total frequent flyer miles accumulated by the customer.
        /// </summary>
        public float Miles { get; set; } = 0;

        /// <summary>
        /// Institutional student or staff identification number issued by the university.
        /// </summary>
        public string CollegeIdNumber { get; set; } = string.Empty;

        /// <summary>
        /// Institutional/university affiliation.
        /// </summary>
        public string College { get; set; } = string.Empty;
    }
}
