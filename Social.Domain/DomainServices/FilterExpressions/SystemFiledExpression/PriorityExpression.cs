using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public class PriorityExpression : OptionExpression
    {
        public PriorityExpression() : base("Priority")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            ConversationPriority value;
            if (Enum.TryParse(condition.Value, out value))
            {
                return t => t.Priority == value;
            }
            else
            {
                return null;
            }
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            ConversationPriority value;
            if (Enum.TryParse(condition.Value, out value))
            {
                return t => t.Priority != value;
            }
            else
            {
                return null;
            }
        }
    }
}
