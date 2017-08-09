using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public class IdPager
    {
        public int MaxNumberOfDataRetrieve { get; set; }
        public int? SinceId { get; set; }
        public int? MaxId { get; set; }
    }
}
