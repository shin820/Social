using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public interface IHaveCreation
    {
        DateTime CreatedTime { get; set; }

        int CreatedBy { get; set; }
    }
}
