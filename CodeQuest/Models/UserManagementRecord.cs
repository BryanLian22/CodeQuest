namespace CodeQuest.Models
{
    /// <summary>
    /// Safe administrator view of a user account. Password hashes are never
    /// included in the admin user-management model.
    /// </summary>
    public sealed class UserManagementRecord
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string Role { get; set; }
        public string Plan { get; set; }
        public string GoogleID { get; set; }
        public int EnrollmentCount { get; set; }
        public int TicketCount { get; set; }
    }
}
