using Moq;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Social.UnitTest.AppServices
{
    public class ConversationMessageAppServiceTest : TestBase
    {
        [Fact]
        public void ShouldGetFacebookDirectMessages()
        {
            //Arrange
            var conversationService = new Mock<IConversationService>();
            var agentService = new Mock<IAgentService>();
            var messageService = new Mock<IMessageService>();
            var twitterService = new Mock<ITwitterService>();
            var socialAccountService = new Mock<ISocialAccountService>();

            ConversationMessageAppService conversationMessageAppService = new ConversationMessageAppService(conversationService.Object,
              agentService.Object, messageService.Object, null, null);

            ConversationAppServiceTest conversationAppServiceTest = new ConversationAppServiceTest();

            conversationService.Setup(t => t.Find(1, ConversationSource.FacebookMessage)).Returns(conversationAppServiceTest.MakeConversationEntity(1));
            messageService.Setup(t => t.FindAllByConversationId(1)).Returns(new List<Message> { MakeMessageEntity(1) }.AsQueryable());

            //Act
            IList<FacebookMessageDto> facebookMessageDtos = conversationMessageAppService.GetFacebookDirectMessages(1);
            //Assert
            Assert.True(facebookMessageDtos.Any());
            agentService.Verify(t => t.FillAgentName(It.Is<IEnumerable<IHaveSendAgent>>(r => r.First().SendAgentId == 1)));
            AssertDtoEqualToEntity(MakeMessageEntity(1), facebookMessageDtos[0]);
        }

        [Fact]
        public void ShouldGetFacebookDirectMessage()
        {
            //Arrange
            var conversationService = new Mock<IConversationService>();
            var agentService = new Mock<IAgentService>();
            var messageService = new Mock<IMessageService>();
            var twitterService = new Mock<ITwitterService>();
            var socialAccountService = new Mock<ISocialAccountService>();

            ConversationMessageAppService conversationMessageAppService = new ConversationMessageAppService(conversationService.Object,
              agentService.Object, messageService.Object, null, null);

            ConversationAppServiceTest conversationAppServiceTest = new ConversationAppServiceTest();

            messageService.Setup(t => t.Find(1)).Returns(MakeMessageEntity(1));
            agentService.Setup(t => t.GetDiaplyName(1)).Returns("a");
            //Act
            FacebookMessageDto facebookMessageDto = conversationMessageAppService.GetFacebookDirectMessage(1);
            //Assert
            Assert.NotNull(facebookMessageDto);
            Assert.Equal("a", facebookMessageDto.SendAgentName);
            AssertDtoEqualToEntity(MakeMessageEntity(1), facebookMessageDto);
        }

        private void AssertDtoEqualToEntity(Message entity, FacebookMessageDto dto)
        {
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.Source, dto.Source);
            Assert.Equal(entity.SenderId, dto.UserId);
            Assert.Equal(entity.SendAgentId, dto.SendAgentId);
        }
        private Message MakeMessageEntity(int id)
        {
            return new Message
            {
                Id = id,
                Source = MessageSource.FacebookMessage,
                SenderId = 1,
                Sender = new SocialUser { Id = 1, Type = SocialUserType.Customer },
                OriginalId = "1",
                SendAgentId = 1
            };
        }
    }
}
