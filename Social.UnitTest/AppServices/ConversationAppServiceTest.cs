﻿using Framework.Core;
using Framework.Core.UnitOfWork;
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
using Xunit;

namespace Social.UnitTest.AppServices
{
    public class ConversationAppServiceTest : TestBase
    {
        public ConversationAppServiceTest()
        {

        }
        [Fact]
        public void ShouldGetDtoById()
        {
            // Arrange
            var fakeConversationEntity = MakeConversationEntity(1);
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns(fakeConversationEntity);
            var messageService = MakeFakeMessageService(fakeConversationEntity.Messages);
            ConversationAppService appSerice = new ConversationAppService(
                conversationService.Object,
                messageService,
                new Mock<IAgentService>().Object,
                new Mock<IDepartmentService>().Object
                , null, null
                );

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
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, messageService.Object, null, null, null, null);

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
            messageService.Setup(t => t.FindAllByConversationIds(It.IsAny<int[]>())).Returns(
             new List<Message> {
                 fakeConversationEntityList.First(t => t.Id == 1).Messages.FirstOrDefault(),
                 fakeConversationEntityList.First(t => t.Id == 2).Messages.FirstOrDefault()
             }.AsQueryable()
            );
            ConversationAppService appSerice = new ConversationAppService(
                conversationService.Object,
                messageService.Object,
                new Mock<IAgentService>().Object,
                new Mock<IDepartmentService>().Object, null, null);

            // Act
            IList<ConversationDto> conversationDtoList = appSerice.Find(new ConversationSearchDto { Until = DateTime.UtcNow });

            // Assert
            Assert.True(conversationDtoList.Any());
            AssertDtoEqualToEntity(fakeConversationEntityList.First(t => t.Id == 2), conversationDtoList[0]);
            AssertDtoEqualToEntity(fakeConversationEntityList.First(t => t.Id == 1), conversationDtoList[1]);
        }

        [Fact]
        public void ShouldInsert()
        {
            // Arrange
            var fakeConversationEntity = MakeConversationEntity(1);
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Insert(It.IsAny<Conversation>())).Returns(fakeConversationEntity);
            var messageService = MakeFakeMessageService(fakeConversationEntity.Messages);
            ConversationAppService appSerice = new ConversationAppService(
                conversationService.Object,
                messageService,
                new Mock<IAgentService>().Object,
                new Mock<IDepartmentService>().Object,
                null, null
                );
            appSerice.UnitOfWorkManager = MakeFakeUnitOfWorkManager();

            // Act
            var dto = appSerice.Insert(new ConversationCreateDto { Subject = fakeConversationEntity.Subject });

            // Assert
            conversationService.Verify(t => t.Insert(It.Is<Conversation>(r => r.Subject == fakeConversationEntity.Subject)));
            Assert.NotNull(dto);
            AssertDtoEqualToEntity(fakeConversationEntity, dto);
        }

        [Fact]
        public void ShouldUpdate()
        {
            // Arrange
            var fakeConversationEntity = MakeConversationEntity(1);
            var notificationService = new Mock<INotificationManager>().Object;
            var messageService = MakeFakeMessageService(fakeConversationEntity.Messages);
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns(fakeConversationEntity);
            ConversationAppService appSerice = new ConversationAppService(
                conversationService.Object,
                messageService, null, null, null,
                notificationService);
            appSerice.UnitOfWorkManager = MakeFakeUnitOfWorkManager();

            // Act
            var dto = appSerice.Update(1, new ConversationUpdateDto());

            // Assert
            conversationService.Verify(t => t.Update(It.Is<Conversation>(r => r.Id == 1)));
            Assert.NotNull(dto);
            AssertDtoEqualToEntity(fakeConversationEntity, dto);
        }

        [Fact]
        public void ShouldThrowExceptionIfConversationNotExistsWhenUpdate()
        {
            // Arrange
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns<Conversation>(null);
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, null, null, null, null, null);

            // Act
            Action action = () => { appSerice.Update(1, new ConversationUpdateDto()); };

            // Assert
            Assert.Throws<ExceptionWithCode>(action);
        }

        [Fact]
        public void ShouldNofityConversationUpdate()
        {
            // Arrange
            var fakeConversationEntity = MakeConversationEntity(1);
            var notificationService = new Mock<INotificationManager>();
            var messageService = MakeFakeMessageService(fakeConversationEntity.Messages);
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns(fakeConversationEntity);
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, messageService, null, null, null, notificationService.Object);
            appSerice.UnitOfWorkManager = MakeFakeUnitOfWorkManager();

            // Act
            var dto = appSerice.Update(1, new ConversationUpdateDto());

            // Assert
            notificationService.Verify(t => t.NotifyUpdateConversation(It.IsAny<int>(), 1));
        }

        [Fact]
        public void ShouldDelete()
        {
            // Arrange
            var fakeConversationEntity = MakeConversationEntity(1);
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns(fakeConversationEntity);
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, null, null, null, null, null);

            // Act
            appSerice.Delete(1);

            // Assert
            conversationService.Verify(t => t.Delete(fakeConversationEntity));
        }

        [Fact]
        public void ShouldThrowExceptionIfConverationNotExistsWhenDelete()
        {
            // Arrange
            var conversationService = new Mock<IConversationService>();
            conversationService.Setup(t => t.Find(1)).Returns<Conversation>(null);
            ConversationAppService appSerice = new ConversationAppService(conversationService.Object, null, null, null, null, null);

            // Act
            Action action = () => { appSerice.Delete(1); };

            // Assert
            Assert.Throws<ExceptionWithCode>(action);
        }

        private IMessageService MakeFakeMessageService(IList<Message> messages)
        {
            var messageServiceMock = new Mock<IMessageService>();
            messageServiceMock.Setup(t => t.FindAll()).Returns(messages.AsQueryable());
            messageServiceMock.Setup(t => t.FindAllByConversationId(It.IsAny<int>())).Returns(messages.AsQueryable());
            messageServiceMock.Setup(t => t.FindAllByConversationIds(It.IsAny<int[]>())).Returns(messages.AsQueryable());
            return messageServiceMock.Object;
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
            Assert.Equal(entity.Messages.First().Content, dto.LastMessage);
            Assert.Equal(entity.Messages.First().Sender.Id, dto.LastIntegrationAccountId);
        }

        public Conversation MakeConversationEntity(int id)
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
                    new Message{
                        ConversationId =id,
                        Content ="Test Message Content",
                        Sender =new SocialUser{
                            Id =1,
                            Type =SocialUserType.IntegrationAccount
                        }
                    }
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