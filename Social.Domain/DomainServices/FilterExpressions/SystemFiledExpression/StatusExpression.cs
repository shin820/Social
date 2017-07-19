using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices
{
    public class StatusExpression : OptionExpression
    {
        public StatusExpression() : base("Status")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            ConversationStatus value;
            if (Enum.TryParse(condition.Value, out value))
            {
                return t => t.Status == value;
            }
            else
            {
                return null;
            }
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            ConversationStatus value;
            if (Enum.TryParse(condition.Value, out value))
            {
                return t => t.Status != value;
            }
            else
            {
                return null;
            }
        }
    }
}
