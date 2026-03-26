namespace FCTournament.Models
{
    public class Organizer
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } // Dùng string vì Id của Identity là string
        public virtual ApplicationUser? User { get; set; }
        public int SubscriptionId { get; set; } = 2;
        public Subscription? Subscriptions { get; set; }
        public int? PendingSubscriptionId { get; set; }
    }
}