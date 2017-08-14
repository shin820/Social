using Framework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class ConversationSearchDto : IdPager
    {
        public int? FilterId { get; set; }
        [MaxLength(100)]
        public string Keyword { get; set; }
        public DateTime? Since { get; set; }
        public DateTime? Util { get; set; }
        [Range(0, int.MaxValue)]
        public int? UserId { get; set; }
    }
}
