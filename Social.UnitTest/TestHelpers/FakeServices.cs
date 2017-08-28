using Moq;
using Social.Domain.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.UnitTest
{
    public static class FakeServices
    {
        public static IAgentService MakeAgentService()
        {
            return new AgentService();
        }

        public static IDepartmentService MakeDepartmentService()
        {
            return new DepartmentService();
        }
    }
}
