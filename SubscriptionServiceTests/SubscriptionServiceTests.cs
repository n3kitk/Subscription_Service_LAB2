
using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Subscription_Service.Models;
using Subscription_Service.Services;
using Subscription_Service.Services.Interfaces;

namespace SubscriptionServiceTests
{
    public class SubscriptionServiceTests
    {
        /// <summary>
        /// Перевіряє, що при відсутності учасника виникає виняток і оплата не перевіряється
        /// </summary>
        [Fact]
        public void RenewSubscription_MemberNotFound_ThrowsException()
        {
            var repo = new Mock<IMemberRepository>();
            var payment = new Mock<IPaymentService>();
            var notify = new Mock<INotificationService>();

            repo.Setup(r => r.GetById(It.IsAny<int>()))
                .Returns((Member?)null);

            var service = new SubscriptionService(
                repo.Object, payment.Object, notify.Object);

            Assert.Throws<ArgumentException>(
                () => service.RenewSubscription(1, 100m, 30));

            payment.Verify(
                p => p.VerifyPayment(It.IsAny<int>(), It.IsAny<decimal>()),
                Times.Never);
        }

        /// <summary>
        /// Перевіряє, що при невдалій оплаті метод повертає false і учасник не оновлюється
        /// </summary>
        [Fact]
        public void RenewSubscription_PaymentFails_ReturnsFalse()
        {
            var repo = new Mock<IMemberRepository>();
            var payment = new Mock<IPaymentService>();
            var notify = new Mock<INotificationService>();

            var member = new Member
            {
                Id = 1,
                IsActive = false
            };

            repo.Setup(r => r.GetById(1)).Returns(member);
            payment.Setup(p => p.VerifyPayment(1, 100m))
                   .Returns(false);

            var service = new SubscriptionService(
                repo.Object, payment.Object, notify.Object);

            var result = service.RenewSubscription(1, 100m, 30);

            Assert.False(result);

            repo.Verify(r => r.Update(It.IsAny<Member>()), Times.Never);
            notify.Verify(
                n => n.SendNotification(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        /// <summary>
        /// Перевіряє успішне поновлення підписки, активацію учасника та надсилання сповіщення
        /// </summary>
        [Fact]
        public void RenewSubscription_Success_UpdatesMember()
        {
            var repo = new Mock<IMemberRepository>();
            var payment = new Mock<IPaymentService>();
            var notify = new Mock<INotificationService>();

            var member = new Member
            {
                Id = 1,
                IsActive = false,
                SubscriptionEnd = DateTime.Now.AddDays(-5)
            };

            var oldDate = member.SubscriptionEnd;

            repo.Setup(r => r.GetById(1)).Returns(member);
            payment.Setup(p => p.VerifyPayment(1, 100m))
                   .Returns(true);

            var service = new SubscriptionService(
                repo.Object, payment.Object, notify.Object);

            var result = service.RenewSubscription(1, 100m, 30);

            Assert.True(result);
            Assert.True(member.IsActive);
            Assert.NotEqual(oldDate, member.SubscriptionEnd);
            Assert.True(member.SubscriptionEnd > DateTime.Now);

            repo.Verify(r => r.Update(member), Times.Exactly(1));

            notify.Verify(
                n => n.SendNotification("Subscription renewed!", 1),
                Times.Exactly(1));
        }

        /// <summary>
        /// Перевіряє деактивацію простроченої підписки та надсилання відповідного сповіщення
        /// </summary>
        [Fact]
        public void DeactivateExpiredMembers_ExpiredMember_IsDeactivated()
        {
            var repo = new Mock<IMemberRepository>();
            var payment = new Mock<IPaymentService>();
            var notify = new Mock<INotificationService>();

            var member = new Member
            {
                Id = 1,
                IsActive = true,
                SubscriptionEnd = DateTime.Now.AddDays(-1)
            };

            repo.Setup(r => r.GetAll())
                .Returns(new List<Member> { member });

            var service = new SubscriptionService(
                repo.Object, payment.Object, notify.Object);

            service.DeactivateExpiredMembers();

            Assert.False(member.IsActive);

            repo.Verify(
                r => r.Update(It.Is<Member>(m => !m.IsActive)),
                Times.Exactly(1));

            notify.Verify(
                n => n.SendNotification("Membership expired", 1),
                Times.Exactly(1));
        }

        /// <summary>
        /// Перевіряє, що учасник із майбутньою датою завершення підписки не деактивується
        /// </summary>
        [Fact]
        public void DeactivateExpiredMembers_ActiveSubscription_IsNotUpdated()
        {
            var repo = new Mock<IMemberRepository>();
            var payment = new Mock<IPaymentService>();
            var notify = new Mock<INotificationService>();

            var member = new Member
            {
                Id = 2,
                IsActive = true,
                SubscriptionEnd = DateTime.Now.AddDays(10)
            };

            repo.Setup(r => r.GetAll())
                .Returns(new List<Member> { member });

            var service = new SubscriptionService(
                repo.Object, payment.Object, notify.Object);

            service.DeactivateExpiredMembers();

            Assert.True(member.IsActive);

            repo.Verify(r => r.Update(It.IsAny<Member>()), Times.Never);
            notify.Verify(
                n => n.SendNotification(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        /// <summary>
        /// Перевіряє, що учасник без дати завершення підписки не деактивується
        /// </summary>
        [Fact]
        public void DeactivateExpiredMembers_NoSubscriptionEnd_DoesNotDeactivateMember()
        {
            var repo = new Mock<IMemberRepository>();
            var payment = new Mock<IPaymentService>();
            var notify = new Mock<INotificationService>();

            var member = new Member
            {
                Id = 3,
                IsActive = true,
                SubscriptionEnd = null
            };

            repo.Setup(r => r.GetAll())
                .Returns(new List<Member> { member });

            var service = new SubscriptionService(
                repo.Object, payment.Object, notify.Object);

            service.DeactivateExpiredMembers();

            Assert.True(member.IsActive);

            repo.Verify(
                r => r.Update(It.IsAny<Member>()),
                Times.Never);

            notify.Verify(
                n => n.SendNotification(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }
    }
}
