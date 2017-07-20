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
    }
}
