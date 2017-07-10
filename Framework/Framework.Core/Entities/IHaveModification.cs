using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public interface IHaveModification
    {
        int? ModifiedBy { get; set; }

        DateTime? ModifiedTime { get; set; }
    }
}
