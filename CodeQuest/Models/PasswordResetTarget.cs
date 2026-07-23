// Purpose: Defines the PasswordResetTarget data shape shared between repositories and Web Forms pages.
using System;

namespace CodeQuest.Models
{
    /// <summary>
    /// Safe details for a token that has passed validation. No token value is
    /// retained in this model after its hash has been checked.
    /// </summary>
    public sealed class PasswordResetTarget
    {
        public int TokenID { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
