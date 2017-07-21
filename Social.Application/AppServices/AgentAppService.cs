using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IAgentAppService
    {
        List<AgentDto> FindAll();
        AgentDto Find(int id);
    }

    public class AgentAppService:AppService,IAgentAppService
    {
        public AgentAppService()
        {
        }

        public List<AgentDto> FindAll()
        {
            List<AgentDto> AgentDtos= new List<AgentDto>();
            AgentDto AgentDto1 = new AgentDto();
            AgentDto1.Id = 1; AgentDto1.Name = "vico"; AgentDto1.Status = 1;
            AgentDtos.Add(AgentDto1);
            AgentDto AgentDto2 = new AgentDto();
            AgentDto2.Id = 2; AgentDto2.Name = "jamm"; AgentDto2.Status = 2;
            AgentDtos.Add(AgentDto2);
            return  AgentDtos ;
        }

        public AgentDto Find(int id)
        {
            List<AgentDto> AgentDtos =FindAll();
            foreach(var AgentDto in AgentDtos)
            {
                if (AgentDto.Id == id)
                {
                    return AgentDto;
                }
            }
            return null;
        }

    }
}
