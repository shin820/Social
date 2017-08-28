using Framework.Core;
using Framework.Core.UnitOfWork;
using Moq;
using Social.Domain.DomainServices;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Social.UnitTest.DomainServices.Facebook.WebHookStrategies
{
    public class NewMessageStrategyTest
    {
        [Fact]
        public void ShouldMatch()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewMessageStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "conversations",
                Value = new FbHookChangeValue
                {
                    ThreadId = "123"
                }
            };

            // Act
            bool isMatch = strategy.IsMatch(fbHookChange);

            // Assert
            Assert.True(isMatch);
        }

        [Fact]
        public void ShouldNotMatch()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewMessageStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "post",
                Value = new FbHookChangeValue
                {
                    ThreadId = "123"
                }
            };

            // Act
            bool isMatch = strategy.IsMatch(fbHookChange);

            // Assert
            Assert.False(isMatch);
        }

        [Fact]
        public async Task ShouldIgnoreNewMessageIfFunctionDisabled()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewMessageStrategy(dependencyResolverMock.Object);
            var socialAccount = new SocialAccount
            {
                IfConvertMessageToConversation = false
            };

            // Act
            var processResult = await strategy.Process(socialAccount, new FbHookChange());

            // Assert
            Assert.False(processResult.NewMessages.Any());
            Assert.False(processResult.NewConversations.Any());
            Assert.False(processResult.UpdatedConversations.Any());
        }

        [Fact]
        public async Task ShouldIngoreDuplicatedMessages()
        {
            // Arrange
            var fbChange = new FbHookChange
            {
                Value = new FbHookChangeValue { ThreadId = "th_1" }
            };
            var fbClientMock = new Mock<IFbClient>();
            fbClientMock.Setup(t => t.GetMessagesFromConversationId("token", "th_1", 10)).ReturnsAsync(
                new List<FbMessage>
                {
                    new FbMessage{Id="fb_1"}
                });
            var messageServiceMock = new Mock<IMessageService>();
            messageServiceMock.Setup(t => t.FindAll()).Returns(new List<Message>
            {
                new Message{Source=MessageSource.FacebookMessage,OriginalId="fb_1"}
            }.AsQueryable());

            var dependencyResolverMock = new Mock<IDependencyResolver>();
            dependencyResolverMock.Setup(t => t.Resolve<IFbClient>()).Returns(fbClientMock.Object);
            dependencyResolverMock.Setup(t => t.Resolve<IMessageService>()).Returns(messageServiceMock.Object);

            var strategy = new NewMessageStrategy(dependencyResolverMock.Object);
            var socialAccount = new SocialAccount { Token = "token", IfConvertMessageToConversation = true };

            // Act
            var processResult = await strategy.Process(socialAccount, fbChange);

            // Assert
            Assert.False(processResult.NewMessages.Any());
            Assert.False(processResult.NewConversations.Any());
            Assert.False(processResult.UpdatedConversations.Any());
        }

        [Fact]
        public async Task ShouldCreateConversation()
        {
            // Arrange
            var fbChange = new FbHookChange
            {
                Value = new FbHookChangeValue { ThreadId = "th_1" }
            };
            var fbClientMock = new Mock<IFbClient>();
            fbClientMock.Setup(t => t.GetMessagesFromConversationId("token", "th_1", 10)).ReturnsAsync(
                new List<FbMessage>
                {
                    new FbMessage{Id="fb_1",SenderId="user_1",ReceiverId="user_2"}
                });
            var messageServiceMock = new Mock<IMessageService>();
            messageServiceMock.Setup(t => t.FindAll()).Returns(new List<Message>().AsQueryable());

            var socialUserServiceMock = new Mock<ISocialUserService>();
            socialUserServiceMock.Setup(t => t.GetOrCreateFacebookUser("token", "user_1")).ReturnsAsync(new SocialUser
            {
                Id = 1
            });
            socialUserServiceMock.Setup(t => t.GetOrCreateFacebookUser("token", "user_2")).ReturnsAsync(new SocialUser
            {
                Id = 2
            });

            var conversationServiceMock = new Mock<IConversationService>();
            conversationServiceMock.Setup(t => t.GetUnClosedConversation("th_1")).Returns<Conversation>(null);

            var dependencyResolverMock = new Mock<IDependencyResolver>();
            dependencyResolverMock.Setup(t => t.Resolve<IFbClient>()).Returns(fbClientMock.Object);
            dependencyResolverMock.Setup(t => t.Resolve<IMessageService>()).Returns(messageServiceMock.Object);
            dependencyResolverMock.Setup(t => t.Resolve<ISocialUserService>()).Returns(socialUserServiceMock.Object);
            dependencyResolverMock.Setup(t => t.Resolve<IConversationService>()).Returns(conversationServiceMock.Object);

            var strategy = new NewMessageStrategy(dependencyResolverMock.Object);
            var uowMock = new Mock<IUnitOfWork>();
            var uowManagerMock = new Mock<IUnitOfWorkManager>();
            uowManagerMock.Setup(t => t.Current).Returns(uowMock.Object);
            strategy.UnitOfWorkManager = uowManagerMock.Object;

            var socialAccount = new SocialAccount { Token = "token", IfConvertMessageToConversation = true };

            // Act
            var processResult = await strategy.Process(socialAccount, fbChange);
            var converssation = processResult.NewConversations.First();
            var message = converssation.Messages.First();

            // Assert
            conversationServiceMock.Verify(t => t.InsertAsync(converssation), "Should insert conversation.");
            uowMock.Verify(t => t.SaveChangesAsync(), "Should save changes after insert.");

            Assert.Equal(1, processResult.NewConversations.Count());
            Assert.NotNull(converssation);
            Assert.Equal("th_1", converssation.OriginalId);
            Assert.Equal(1, converssation.Messages.Count());
            Assert.Equal("fb_1", message.OriginalId);
            Assert.Equal(1, message.SenderId);
            Assert.Equal(2, message.ReceiverId);
        }

        //[Fact]
        //public async Task ShouldCreateConversationWithMessages()
        //{
        //    // Arrange
        //    var fbChange = new FbHookChange
        //    {
        //        Value = new FbHookChangeValue { ThreadId = "th_1" }
        //    };
        //    var fbClientMock = new Mock<IFbClient>();
        //    fbClientMock.Setup(t => t.GetMessagesFromConversationId("token", "th_1", 10)).ReturnsAsync(
        //        new List<FbMessage>
        //        {
        //            new FbMessage{Id="fb_1",SenderId="user_1",ReceiverId="user_2"},
        //            new FbMessage{Id="fb_2",SenderId="user_2",ReceiverId="user_1"}
        //        });
        //    var messageServiceMock = new Mock<IMessageService>();
        //    messageServiceMock.Setup(t => t.FindAll()).Returns(new List<Message>().AsQueryable());

        //    var socialUserServiceMock = new Mock<ISocialUserService>();
        //    socialUserServiceMock.Setup(t => t.GetOrCreateFacebookUser("token", "user_1")).ReturnsAsync(new SocialUser
        //    {
        //        Id = 1
        //    });
        //    socialUserServiceMock.Setup(t => t.GetOrCreateFacebookUser("token", "user_2")).ReturnsAsync(new SocialUser
        //    {
        //        Id = 2
        //    });

        //    var conversationServiceMock = new Mock<IConversationService>();
        //    conversationServiceMock.Setup(t => t.GetUnClosedConversation("th_1")).Returns<Conversation>(null);

        //    var dependencyResolverMock = new Mock<IDependencyResolver>();
        //    dependencyResolverMock.Setup(t => t.Resolve<IFbClient>()).Returns(fbClientMock.Object);
        //    dependencyResolverMock.Setup(t => t.Resolve<IMessageService>()).Returns(messageServiceMock.Object);
        //    dependencyResolverMock.Setup(t => t.Resolve<ISocialUserService>()).Returns(socialUserServiceMock.Object);
        //    dependencyResolverMock.Setup(t => t.Resolve<IConversationService>()).Returns(conversationServiceMock.Object);

        //    var strategy = new NewMessageStrategy(dependencyResolverMock.Object);
        //    var uowMock = new Mock<IUnitOfWork>();
        //    var uowManagerMock = new Mock<IUnitOfWorkManager>();
        //    uowManagerMock.Setup(t => t.Current).Returns(uowMock.Object);
        //    strategy.UnitOfWorkManager = uowManagerMock.Object;

        //    var socialAccount = new SocialAccount { Token = "token", IfConvertMessageToConversation = true };

        //    // Act
        //    var processResult = await strategy.Process(socialAccount, fbChange);
        //    var converssation = processResult.NewConversations.First();
        //    var firstMessage = converssation.Messages[0];
        //    var secondMessage = converssation.Messages[1];

        //    // Assert
        //    conversationServiceMock.Verify(t => t.InsertAsync(converssation), "Should insert conversation.");
        //    uowMock.Verify(t => t.SaveChangesAsync(), "Should save changes after insert.");

        //    Assert.Equal(1, processResult.NewConversations.Count());
        //    Assert.NotNull(converssation);
        //    Assert.Equal("th_1", converssation.OriginalId);
        //    Assert.Equal(2, converssation.Messages.Count());
        //    Assert.Equal("fb_1", firstMessage.OriginalId);
        //    Assert.Equal(1, firstMessage.SenderId);
        //    Assert.Equal(2, firstMessage.ReceiverId);
        //    Assert.Equal("fb_2", secondMessage.OriginalId);
        //    Assert.Equal(2, secondMessage.SenderId);
        //    Assert.Equal(1, secondMessage.ReceiverId);
        //}
    }
}
