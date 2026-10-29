using Subscription_Service.Models;

namespace Subscription_Service.Services.Interfaces
{
    public interface IMemberRepository
    {
        Member? GetById(int id);
        IEnumerable<Member> GetAll();
        void Update(Member member);

    }
}
