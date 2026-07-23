// Purpose: Defines the TicketRecord data shape shared between repositories and Web Forms pages.
using System;
using System.Collections.Generic;

namespace CodeQuest.Models
{
    /// <summary>
    /// A support ticket from dbo.Ticket, optionally including its replies.
    /// </summary>
    public sealed class TicketRecord
    {
        public int TicketID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ReplyCount { get; set; }
        public DateTime? LastReplyAt { get; set; }
        public IList<TicketReplyRecord> Replies { get; set; }
    }
}
