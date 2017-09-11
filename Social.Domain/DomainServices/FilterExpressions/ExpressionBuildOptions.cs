using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public class ExpressionBuildOptions
    {
        public int UserId { get; set; }
        public int[] MyDepartmentMembers { get; set; }
        public int MyDepartmentId { get; set; }
        public IList<DepartmentStatus> DepartmentStatuses { get; set; }
    }
}
