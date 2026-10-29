using Moq;
using Xunit;
using Subscription_Service.Models;
using Subscription_Service.Services;
using Subscription_Service.Services.Interfaces;

namespace SubscriptionServiceTests
{
    public class MemberServiceTests
    {
        /// <summary>
        /// Перевіряє статус учасника для активного та неактивного станів
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsActive_ReturnsCorrectStatus(bool expected)
        {
            var repo = new Mock<IMemberRepository>();

            var member = new Member
            {
                Id = 1,
                IsActive = expected
            };

            repo.Setup(r => r.GetById(1)).Returns(member);

            var service = new MemberService(repo.Object);

            var result = service.IsActive(1);

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Перевіряє, що для неіснуючого учасника метод IsActive повертає false
        /// </summary>
        [Fact]
        public void IsActive_MemberNotFound_ReturnsFalse()
        {
            var repo = new Mock<IMemberRepository>();

            repo.Setup(r => r.GetById(99))
                .Returns((Member?)null);

            var service = new MemberService(repo.Object);

            var result = service.IsActive(99);

            Assert.False(result);
        }

        /// <summary>
        /// Перевіряє, що метод GetMember повертає null,у тому випадку якщо учасника не знайдено
        /// </summary>
        [Fact]
        public void GetMember_NotFound_ReturnsNull()
        {
            var repo = new Mock<IMemberRepository>();

            repo.Setup(r => r.GetById(99))
                .Returns((Member?)null);

            var service = new MemberService(repo.Object);

            var result = service.GetMember(99);

            Assert.Null(result);
        }

        /// <summary>
        /// Перевіряє, що метод GetMember повертає знайденого учасника
        /// </summary>
        [Fact]
        public void GetMember_Found_ReturnsMember()
        {
            var repo = new Mock<IMemberRepository>();

            var member = new Member
            {
                Id = 1,
                IsActive = true
            };

            repo.Setup(r => r.GetById(1)).Returns(member);

            var service = new MemberService(repo.Object);

            var result = service.GetMember(1);

            Assert.NotNull(result);
            Assert.Same(member, result);
        }
    }
}