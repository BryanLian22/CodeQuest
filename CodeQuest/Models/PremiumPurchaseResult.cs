namespace CodeQuest.Models
{
    /// <summary>
    /// Result of the demo Premium checkout transaction.
    /// </summary>
    public sealed class PremiumPurchaseResult
    {
        public bool AlreadyPremium { get; set; }
        public int SubscriptionID { get; set; }
        public string TransactionReference { get; set; }
    }
}
