using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure.Enum
{
    public enum ConversationStatus : short
    {
        New = 0,
        PendingInternal = 1,
        PendingExternal = 2,
        OnHold = 3,
        Closed = 4
    }
}
