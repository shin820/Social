using Framework.Core;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.Entities
{
    [Table("t_Social_Conversation")]
    public class Conversation : EntityWithSite, IHaveModification
    {
        public Conversation()
        {
            Messages = new List<Message>();
            Logs = new List<ConversationLog>();
            Filters = new List<Filter>();
            Fields = new List<ConversationField>();
        }

        public ConversationSource Source { get; set; }

        public string SocialId { get; set; }

        public bool IfRead { get; set; }

        public DateTime LastMessageSentTime { get; set; }

        public int LastMessageSenderId { get; set; }

        public int? LastRepliedAgentId { get; set; }

        public int? AgentId { get; set; }

        public int? DepartmentId { get; set; }

        public ConversationStatus Status { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }

        public ConversationPriority Priority { get; set; }

        [MaxLength(2000)]
        public string Note { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public virtual IList<Message> Messages { get; set; }

        public virtual IList<ConversationLog> Logs { get; set; }

        public virtual IList<Filter> Filters { get; set; }

        public virtual IList<ConversationField> Fields { get; set; }

        public virtual SocialUser LastMessageSender { get; set; }
    }
}
