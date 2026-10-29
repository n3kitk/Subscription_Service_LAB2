namespace Subscription_Service.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
    }
}
