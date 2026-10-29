namespace Subscription_Service.Services.Interfaces
{
    public interface IPaymentService
    {
        bool VerifyPayment(int memberId, decimal amount);

    }
}
