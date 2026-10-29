using Subscription_Service.Services.Interfaces;

namespace Subscription_Service.Services
{
    public class SubscriptionService
    {
        private readonly IMemberRepository _repo;
        private readonly IPaymentService _payment;
        private readonly INotificationService _notify;

        public SubscriptionService(IMemberRepository repo, IPaymentService payment, INotificationService notify)
        {
            _repo = repo;
            _payment = payment;
            _notify = notify;
        }

        public bool RenewSubscription(int memberId, decimal paymentAmount, int days)
        {
            var member = _repo.GetById(memberId);
            if (member == null)
                throw new ArgumentException("Member not found");

            if (!_payment.VerifyPayment(memberId, paymentAmount))
                return false;

            member.SubscriptionEnd = DateTime.Now.AddDays(days);
            member.IsActive = true;
            _repo.Update(member);
            _notify.SendNotification("Subscription renewed!", memberId);

            return true;
        }

        public void DeactivateExpiredMembers()
        {
            var members = _repo.GetAll();
            foreach (var m in members)
            {
                if (m.SubscriptionEnd.HasValue && m.SubscriptionEnd < DateTime.Now)
                {
                    m.IsActive = false;
                    _repo.Update(m);
                    _notify.SendNotification("Membership expired", m.Id);
                }
            }
        }
    }

}
