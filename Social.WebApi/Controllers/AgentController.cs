using Social.Application.AppServices;
using Social.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("agents")]
    public class AgentController:ApiController
    {
        private IAgentAppService _agentService;

        public AgentController(IAgentAppService agentService)
        {
            _agentService = agentService;
        }

        [Route()]
        public List<AgentDto> GetAgents()
        {
            return _agentService.FindAll();
        }

        [Route("{id}", Name = "GetAgent")]
        public AgentDto GetAgent(int id)
        {
            return _agentService.Find(id);
        }
    }
}