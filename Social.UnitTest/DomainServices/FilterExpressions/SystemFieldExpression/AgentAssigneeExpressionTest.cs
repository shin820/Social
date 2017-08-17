using Framework.Core;
using Moq;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Social.UnitTest.DomainService.FilterExpressions.SystemFieldExpression
{
    public class AgentAssigneeExpressionTest
    {
        [Fact]
        public void ShouldFilterByIsCondition()
        {
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(t => t.UserId).Returns(1);

            var deparmentMock = new Mock<IDepartmentService>();
            deparmentMock.Setup(t => t.GetMyDepartmentMembers(1)).Returns(new[] { 1, 2, 3 });


            FilterCondition condition = new FilterCondition
            {
                Field = new ConversationField { Name = "Agent Assignee", DataType = FieldDataType.Option },
                MatchType = ConditionMatchType.Is,
                Value = "@Me"
            };
            var expression = new AgentAssigneeExpression(userContextMock.Object, deparmentMock.Object).Build(condition);

            var conditions = new List<Conversation>
            {
                new Conversation{Id=1,AgentId=1},
                new Conversation{Id=2,AgentId=null}
            }.AsQueryable();

            var result = conditions.Where(expression).ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public void ShouldFilterByIsNotCondition()
        {
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(t => t.UserId).Returns(1);

            var deparmentMock = new Mock<IDepartmentService>();
            deparmentMock.Setup(t => t.GetMyDepartmentMembers(1)).Returns(new[] { 1, 2, 3 });


            FilterCondition condition = new FilterCondition
            {
                Field = new ConversationField { Name = "Agent Assignee", DataType = FieldDataType.Option },
                MatchType = ConditionMatchType.IsNot,
                Value = "@Me"
            };
            var expression = new AgentAssigneeExpression(userContextMock.Object, deparmentMock.Object).Build(condition);

            var conditions = new List<Conversation>
            {
                new Conversation{Id=1,AgentId=1},
                new Conversation{Id=2,AgentId=null}
            }.AsQueryable();

            var result = conditions.Where(expression).ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result.First().Id);
        }
    }
}
