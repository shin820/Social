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
    public interface IDepartmentAppService
    {
        List<DepartmentDto> FindAll();
        DepartmentDto Find(int id);
    }

    public class DepartmentAppService : AppService, IDepartmentAppService
    {
        public DepartmentAppService()
        {
        }

        public List<DepartmentDto> FindAll()
        {
            List<DepartmentDto> DepartmentDtos = new List<DepartmentDto>();
            DepartmentDto DepartmentDto1 = new DepartmentDto();
            DepartmentDto1.Id = 1; DepartmentDto1.Name = "Sales"; 
            DepartmentDtos.Add(DepartmentDto1);
            DepartmentDto DepartmentDto2 = new DepartmentDto();
            DepartmentDto2.Id = 2; DepartmentDto2.Name = "Support"; 
            DepartmentDtos.Add(DepartmentDto2);
            return DepartmentDtos;
        }

        public DepartmentDto Find(int id)
        {
            List<DepartmentDto> DepartmentDtos = FindAll();
            foreach (var DepartmentDto in DepartmentDtos)
            {
                if (DepartmentDto.Id == id)
                {
                    return DepartmentDto;
                }
            }
            return null;
        }

    }
}
