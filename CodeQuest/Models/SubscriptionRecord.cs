// Purpose: Defines the SubscriptionRecord data shape shared between repositories and Web Forms pages.
using System;

namespace CodeQuest.Models
{
    /// <summary>
    /// A subscription joined with the plan information stored in dbo.Subscription.
    /// </summary>
    public sealed class SubscriptionRecord
    {
        public int SubscriptionID { get; set; }
        public int UserID { get; set; }
        public string PlanType { get; set; }
        public string BillingCycle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }

        public bool IsActive
        {
            get { return string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
