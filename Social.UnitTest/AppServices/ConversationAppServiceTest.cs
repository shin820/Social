using Framework.Core;
using Moq;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Social.UnitTest.AppServices
{
    public class ConversationAppServiceTest : TestBase
    {
        [Fact]
        public void ShouldGetDtoById()
        {
            // Arrange
            var fakeConversationEntity = MakeConversationEntity(1);
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns(fakeConversationEntity);
            var messageService = new Mock<IMessageService>();
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, null, null, messageService.Object);

            // Act
            ConversationDto conversationDto = appSerice.Find(1);

            // Assert
            Assert.NotNull(conversationDto);
            AssertDtoEqualToEntity(fakeConversationEntity, conversationDto);
        }

        [Fact]
        public void ShouldThrowExceptionIfConverationNotFoundWhenFindById()
        {
            // Arrange
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns<Conversation>(null);
            var messageService = new Mock<IMessageService>();
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, null, null, messageService.Object);

            // Act
            Action action = () => { appSerice.Find(1); };

            // Assert
            Assert.Throws<ExceptionWithCode>(action);
        }

        [Fact]
        public void ShouldGetDtoList()
        {
            // Arrange
            var fakeConversationEntityList = new List<Conversation>
            {
                MakeConversationEntity(1),
                MakeConversationEntity(2)
            }.AsQueryable();
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.FindAll()).Returns(fakeConversationEntityList);
            conversationService.Setup(t => t.ApplyFilter(fakeConversationEntityList, It.IsAny<int?>())).Returns(fakeConversationEntityList);
            conversationService.Setup(t => t.ApplyKeyword(fakeConversationEntityList, It.IsAny<string>())).Returns(fakeConversationEntityList);
            conversationService.Setup(t => t.ApplySenderOrReceiverId(fakeConversationEntityList, It.IsAny<int?>())).Returns(fakeConversationEntityList);
            var messageService = new Mock<IMessageService>();
            messageService.Setup(t => t.GetLastMessages(It.IsAny<int[]>())).Returns(
             new List<Message> {
                 fakeConversationEntityList.First(t => t.Id == 1).Messages.FirstOrDefault(),
                 fakeConversationEntityList.First(t => t.Id == 2).Messages.FirstOrDefault()
             }
            );
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, null, null, messageService.Object);

            // Act
            IList<ConversationDto> conversationDtoList = appSerice.Find(new ConversationSearchDto { Util = DateTime.UtcNow });

            // Assert
            Assert.True(conversationDtoList.Any());
            AssertDtoEqualToEntity(fakeConversationEntityList.First(t => t.Id == 2), conversationDtoList[0]);
            AssertDtoEqualToEntity(fakeConversationEntityList.First(t => t.Id == 1), conversationDtoList[1]);
        }

        private void AssertDtoEqualToEntity(Conversation entity, ConversationDto dto)
        {
            Assert.NotNull(dto);
            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.Subject, dto.Subject);
            Assert.Equal(entity.Note, dto.Note);
            Assert.Equal(entity.Status, dto.Status);
            Assert.Equal(entity.OriginalId, dto.OriginalId);
            Assert.Equal(entity.AgentId, dto.AgentId);
            Assert.Equal(entity.DepartmentId, dto.DepartmentId);
            Assert.Equal(entity.IfRead, dto.IfRead);
            Assert.Equal(entity.LastMessageSenderId, dto.LastMessageSenderId);
            Assert.Equal(entity.LastMessageSender.Name, dto.LastMessageSenderName);
            Assert.Equal(entity.LastMessageSender.Avatar, dto.LastMessageSenderAvatar);
            Assert.Equal(entity.LastMessageSentTime, dto.LastMessageSentTime);
            Assert.Equal(entity.Priority, dto.Priority);
        }

        private Conversation MakeConversationEntity(int id)
        {
            return new Conversation
            {
                Id = id,
                Subject = "Test Conversation Subject",
                Note = "Test Conversation Note",
                Status = ConversationStatus.New,
                OriginalId = new Guid().ToString(),
                AgentId = 1,
                DepartmentId = 1,
                IfRead = true,
                Messages = new List<Message>
                {
                    new Message{ConversationId=id,Content="Test Message Content"}
                },
                LastMessageSenderId = 1,
                LastMessageSentTime = DateTime.UtcNow,
                LastMessageSender = new SocialUser
                {
                    Id = 1,
                    Name = "Test User",
                    Avatar = "http://test"
                },
                Priority = ConversationPriority.High,
                CreatedTime = DateTime.UtcNow
            };
        }
    }
}
