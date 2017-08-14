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
        /// <summary>
        /// Filter Id
        /// </summary>
        public int? FilterId { get; set; }

        /// <summary>
        /// Subject, note, message, sender or ricpient.
        /// </summary>
        [MaxLength(100)]
        public string Keyword { get; set; }

        /// <summary>
        /// The result will contains conversations which create time greater than Since date.
        /// </summary>
        public DateTime? Since { get; set; }


        /// <summary>
        /// The result will contains conversations which create time less or equal to Util date.
        /// </summary>
        public DateTime? Util { get; set; }
        [Range(0, int.MaxValue)]
        public int? UserId { get; set; }
    }
}
