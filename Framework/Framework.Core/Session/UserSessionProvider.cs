using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Session
{
    public class UserSessionProvider : IUserSessionProvider
    {
        public int? GetUserId()
        {
            return 1;
        }
    }
}
