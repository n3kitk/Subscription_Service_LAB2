namespace Subscription_Service.Services.Interfaces
{
    public interface INotificationService
    {
        void SendNotification(string message, int memberId);

    }
}
