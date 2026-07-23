// Purpose: Defines the PasswordResetIssue data shape shared between repositories and Web Forms pages.
using System;

namespace CodeQuest.Models
{
    /// <summary>
    /// A one-time password-reset invitation. The raw token is returned only
    /// long enough for the page to build a reset URL; dbo.Token stores its hash.
    /// </summary>
    public sealed class PasswordResetIssue
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string RawToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
