// Purpose: Defines the PaymentRecord data shape shared between repositories and Web Forms pages.
using System;

namespace CodeQuest.Models
{
    /// <summary>
    /// A payment record stored in dbo.Payment.
    /// </summary>
    public sealed class PaymentRecord
    {
        public int PaymentID { get; set; }
        public int UserID { get; set; }
        public int? SubscriptionID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
