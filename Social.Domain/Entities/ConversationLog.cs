using Framework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.Entities
{
    [Table("t_Social_ConversationLog")]
    public class ConversationLog : EntityWithSite, IHaveCreation
    {
        public int ConversationId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedBy { get; set; }

        public virtual Conversation Conversation { get; set; }
    }
}
