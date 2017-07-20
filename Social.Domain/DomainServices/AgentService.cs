using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface IAgentService
    {
        string GetDiaplyName(int? id);
    }

    public class AgentService : ITransient, IAgentService
    {
        public string GetDiaplyName(int? id)
        {
            if (id == null)
            {
                return "N/A";
            }

            return "Test Agent";
        }
    }
}
