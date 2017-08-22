using Framework.Core;
using Moq;
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

namespace Social.UnitTest.DomainServices
{
    public class ConversationServiceTest
    {
        [Fact]
        public void ShouldCheckIfExistsWorks()
        {
            // Arrange
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            var conversationRepo = new Mock<IRepository<Conversation>>();
            conversationRepo.Setup(t => t.FindAll()).Returns(new List<Conversation> { new Conversation { Id = 1, Subject = "Conversation Test 1",IsDeleted = true } }.AsQueryable());

            var conversationService = new ConversationService(agentService.Object,departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            conversationService.Repository = conversationRepo.Object;

            // Act
            Action action = () => { conversationService.CheckIfExists(1); };
            // Assert
            Assert.Throws<ExceptionWithCode>( action);
        }

        public void ShouldApplyKeywordWorks()
        {
            // Arrange
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            var conversationService = new ConversationService(agentService.Object, departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            var rawConversations =  new List<Conversation>
            {
                new Conversation { Id = 1, Subject = "Conversation Test 1" },
                new Conversation { Id = 2, Subject ="",Note= "Conversation2" }
            }.AsQueryable();
            // Act
            var conversations = conversationService.ApplyKeyword(rawConversations, "Conversation").DefaultIfEmpty().ToList();
            // Assert
            Assert.Equal(2, conversations.Count());
        }

        [Fact]
        public void ShouldSenderOrReceiverIdWorks()
        {
            // Arrange
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            var conversationService = new ConversationService(agentService.Object, departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            var rawConversations = new List<Conversation>
            {
                new Conversation { Id = 1, Subject = "Conversation Test 1" ,Messages =new List<Message>{ new Message { SenderId = 1 } }},
                new Conversation { Id = 2, Note = "Conversation2" ,Messages =new List<Message>{ new Message {ReceiverId = 1} }}
            }.AsQueryable();
            // Act
            var conversations = conversationService.ApplySenderOrReceiverId(rawConversations, 1).DefaultIfEmpty().ToList();
            // Assert
            Assert.Equal(2, conversations.Count());
        }

        [Fact]
        public void ShouldApplyFilterWorks()
        {
            // Arrange
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            
             var conversationService = new ConversationService(agentService.Object, departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            var rawConversations = new List<Conversation>
            {
                new Conversation { Id = 1, Subject = "Conversation Test 1" },
                new Conversation { Id = 2, Subject = "test2"}
            }.AsQueryable();
            Filter filter = new Filter
            {
                Id = 1,
                IfPublic = true,
                Index = 1,
                Type = FilterType.All,
                Conditions = new List<FilterCondition>
                {
                    new FilterCondition{FilterId = 1,MatchType = ConditionMatchType.Is ,Value = "Conversation"}
                }
            };
            filterExpressionFactory.Setup(t => t.Create(filter)).Returns(t => t.Subject.Contains("Conversation"));
            // Act
            var conversations = conversationService.ApplyFilter(rawConversations, filter).DefaultIfEmpty().ToList();
            // Assert
            Assert.Equal(1, conversations.Count());
        }
    }
}
