using Framework.Core;
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
    }

    public class DepartmentService : ITransient, IDepartmentService
    {
        public string GetDisplayName(int? departmentId)
        {
            if (departmentId == null)
            {
                return "N/A";
            }

            return "Test Department";
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
