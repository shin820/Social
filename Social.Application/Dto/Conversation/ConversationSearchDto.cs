using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class ConversationSearchDto : IdPager
    {
        public int? FilterId { get; set; }
        public string Keyword { get; set; }
        public DateTime? Since { get; set; }
        public DateTime? Util { get; set; }
    }
}
