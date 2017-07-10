using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure.Enum
{
    public enum ConditionRuleTriggerType : short
    {
        Any = 0,
        All = 1,
        LogicalExpression = 2
    }
}
