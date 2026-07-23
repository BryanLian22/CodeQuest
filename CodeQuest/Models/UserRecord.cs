// Purpose: Defines the UserRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    /// <summary>
    /// Application representation of dbo.User from the CodeQuest ERD.
    /// PasswordHash contains the value stored in dbo.User.password.
    /// </summary>
    public sealed class UserRecord
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string Role { get; set; }
        public string Plan { get; set; }
        public string GoogleID { get; set; }
    }
}
