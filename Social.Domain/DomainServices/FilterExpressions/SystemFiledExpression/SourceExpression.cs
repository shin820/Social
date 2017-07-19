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
    public class SourceExpression : OptionExpression
    {
        public SourceExpression() : base("Source")
        {
        }

        protected override Expression<Func<Conversation, bool>> Is(FilterCondition condition)
        {
            ConversationSource value;
            if (Enum.TryParse(condition.Value, out value))
            {
                return t => t.Source == value;
            }
            else
            {
                return null;
            }
        }

        protected override Expression<Func<Conversation, bool>> IsNot(FilterCondition condition)
        {
            ConversationSource value;
            if (Enum.TryParse(condition.Value, out value))
            {
                return t => t.Source != value;
            }
            else
            {
                return null;
            }
        }
    }
}
