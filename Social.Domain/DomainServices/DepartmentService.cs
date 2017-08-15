using Framework.Core;
using Social.Domain.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface IDepartmentService
    {
        string GetDisplayName(int? departmentId);
        int GetMyDepartmentId(int userId);
        int[] GetMyDepartmentMembers(int userId);
        int[] IsMatchStatusDepartments(int status);
        IQueryable<Department> FindAll();
        Department Find(int id);
    }

    public class DepartmentService : ITransient, IDepartmentService
    {
        public IQueryable<Department> FindAll()
        {
            var departments = new List<Department>
            {
                new Department{Id=1,Name="Test Department 1"},
                new Department{Id=2,Name="Test Department 2"},
                new Department{Id=3,Name="Test Department 3"},
                new Department{Id=4,Name="Test Department 4"},
                new Department{Id=5,Name="Test Department 5"},
            };

            return departments.AsQueryable();
        }

        public Department Find(int id)
        {
            return FindAll().FirstOrDefault(t => t.Id == id);
        }


        public string GetDisplayName(int? departmentId)
        {
            if (departmentId == null)
            {
                return "N/A";
            }

            var department = Find(departmentId.Value);
            return department == null ? "N/A" : department.Name;
        }

        public int GetMyDepartmentId(int userId)
        {
            if (userId == null)
            {
                return 0;
            }

            return 1;
        }

        public int[] GetMyDepartmentMembers(int userId)
        {
            if (userId == null)
            {
                return new int[] { };
            }

            return new int[] { };
        }

        public int[] GetOfflineMembers(int[] agents)
        {
            return new int[] { };
        }

        public int[] IsMatchStatusDepartments(int status)
        {
            return new int[] { };
        }
    }
}
