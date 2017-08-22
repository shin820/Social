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
                new Conversation { Id = 2, Note = "Conversation2" },
                new Conversation { Id = 3, Messages =new List<Message>{ new Message { Content = "Conversation3's message" } } },
            };
            var conversations = conversationService.ApplyKeyword(rawConversations.AsQueryable(), "Conversation").ToList();
            Assert.Equal(3, conversations.Count());
        }
    }
}
