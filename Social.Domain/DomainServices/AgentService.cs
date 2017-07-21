using Framework.Core;
using Social.Domain.Entities.General;
using Social.Infrastructure;
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
        void FillAgentName(IEnumerable<IHaveSendAgent> list);
        int[] IsMatchStatusAgents(int status);
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

        public void FillAgentName(IEnumerable<IHaveSendAgent> list)
        {
            List<int> agentIds = list.Where(t => t.SendAgentId != null).Select(t => t.SendAgentId.Value).Distinct().ToList();
            if (agentIds.Any())
            {
                //todo
                foreach (var item in list)
                {
                    item.SendAgentName = "Test Agent";
                }
            }
        }

        public int[] IsMatchStatusAgents(int status)
        {
            return new int[] { };

        }
    }
}
