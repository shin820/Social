using Framework.Core;
using Moq;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
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
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            var conversationRepo = new Mock<IRepository<Conversation>>();
            conversationRepo.Setup(t => t.FindAll()).Returns(new List<Conversation> { new Conversation { Id = 1, Subject = "Conversation Test 1",IsDeleted = true } }.AsQueryable());

            var conversationService = new ConversationService(agentService.Object,departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            conversationService.Repository = conversationRepo.Object;
            try
            {
                var conversation = conversationService.CheckIfExists(1);
                Assert.Equal(1, conversation.Id);
            }
            catch(Exception ex)
            {
                Assert.Equal($"Conversation '1' not exists.", ex.Message);
            }
        }

        public void ShouldApplyKeywordWorks()
        {
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            var conversationRepo = new Mock<IRepository<Conversation>>();
            conversationRepo.Setup(t => t.FindAll()).Returns(new List<Conversation> { new Conversation { Id = 1, Subject = "Conversation Test 1", IsDeleted = true } }.AsQueryable());

            var conversationService = new ConversationService(agentService.Object, departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            conversationService.Repository = conversationRepo.Object;
            var rawConversations =  new List<Conversation>
            {
                new Conversation { Id = 1, Subject = "Conversation Test 1" },
                new Conversation { Id = 2, Subject ="",Note= "Conversation2" }
            }.AsQueryable();
            var conversations = conversationService.ApplyKeyword(rawConversations, "Conversation").DefaultIfEmpty().ToList();
            Assert.Equal(2, conversations.Count());
        }

        [Fact]
        public void ShouldSenderOrReceiverIdWorks()
        {
            var agentService = new Mock<IAgentService>();
            var departmentService = new Mock<IDepartmentService>();
            var filterRepo = new Mock<IRepository<Filter>>();
            var filterExpressionFactory = new Mock<IFilterExpressionFactory>();
            var logRepo = new Mock<IRepository<ConversationLog>>();

            var conversationRepo = new Mock<IRepository<Conversation>>();
            conversationRepo.Setup(t => t.FindAll()).Returns(new List<Conversation> { new Conversation { Id = 1, Subject = "Conversation Test 1", IsDeleted = true } }.AsQueryable());

            var conversationService = new ConversationService(agentService.Object, departmentService.Object, filterRepo.Object, filterExpressionFactory.Object, logRepo.Object);
            conversationService.Repository = conversationRepo.Object;
            var rawConversations = new List<Conversation>
            {
                new Conversation { Id = 1, Subject = "Conversation Test 1" ,Messages =new List<Message>{ new Message { SenderId = 1 } }},
                new Conversation { Id = 2, Note = "Conversation2" ,Messages =new List<Message>{ new Message {ReceiverId = 1} }}
            }.AsQueryable();

            var conversations = conversationService.ApplySenderOrReceiverId(rawConversations, 1).DefaultIfEmpty().ToList();
            Assert.Equal(2, conversations.Count());
        }
    }
}
