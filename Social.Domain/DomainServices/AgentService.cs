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
        void FillCreatedByName(IEnumerable<IHaveCreatedBy> list);
        int[] IsMatchStatusAgents(int status);
        IQueryable<Agent> FindAll();
        Agent Find(int id);
    }

    public class AgentService : ITransient, IAgentService
    {
        public IQueryable<Agent> FindAll()
        {
            var agents = new List<Agent>
            {
                new Agent { Id=1,Name="Test Agent 1"},
                new Agent { Id=2,Name="Test Agent 2"},
                new Agent { Id=3,Name="Test Agent 3"},
                new Agent { Id=3,Name="Test Agent 4"},
                new Agent { Id=3,Name="Test Agent 5"},
                new Agent { Id=3,Name="Test Agent 6"},
                new Agent { Id=3,Name="Test Agent 7"},
                new Agent { Id=3,Name="Test Agent 8"},
                new Agent { Id=3,Name="Test Agent 9"},
                new Agent { Id=3,Name="Test Agent 10"},
            };

            return agents.AsQueryable();
        }

        public Agent Find(int id)
        {
            return FindAll().FirstOrDefault(t => t.Id == id);
        }

        public string GetDiaplyName(int? id)
        {
            if (id == null)
            {
                return "N/A";
            }

            var agent = Find(id.Value);
            return agent == null ? "N/A" : agent.Name;
        }

        public void FillAgentName(IEnumerable<IHaveSendAgent> list)
        {
            List<int> agentIds = list.Where(t => t.SendAgentId != null).Select(t => t.SendAgentId.Value).Distinct().ToList();
            if (agentIds.Any())
            {
                var agents = FindAll().ToList();
                foreach (var item in list)
                {
                    var agent = agents.FirstOrDefault(t => t.Id == item.SendAgentId);
                    if (agent != null)
                    {
                        item.SendAgentName = agent.Name;
                    }
                }
            }
        }

        public void FillCreatedByName(IEnumerable<IHaveCreatedBy> list)
        {
            List<int> agentIds = list.Select(t => t.CreatedBy).Distinct().ToList();
            if (agentIds.Any())
            {
                var agents = FindAll().ToList();
                foreach (var item in list)
                {
                    var agent = agents.FirstOrDefault(t => t.Id == item.CreatedBy);
                    if (agent != null)
                    {
                        item.CreatedByName = agent.Name;
                    }
                }
            }
        }

        public int[] IsMatchStatusAgents(int status)
        {
            return new int[] { };

        }

        public bool ChechAgentStatus(int id, int status)
        {
            return false;
        }
    }
}
