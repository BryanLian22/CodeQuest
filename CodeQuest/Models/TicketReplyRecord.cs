using System;

namespace CodeQuest.Models
{
    /// <summary>
    /// A support reply joined with the author's display name.
    /// </summary>
    public sealed class TicketReplyRecord
    {
        public int ReplyID { get; set; }
        public int TicketID { get; set; }
        public int UserID { get; set; }
        public string AuthorName { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }
    }
}
