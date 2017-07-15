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
    public class Conversation : EntityWithSite, ISoftDelete
    {
        public Conversation()
        {
            Messages = new List<Message>();
            Logs = new List<ConversationLog>();
        }

        public ConversationSource Source { get; set; }

        [MaxLength(200)]
        public string OriginalId { get; set; }

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

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public virtual IList<Message> Messages { get; set; }

        public virtual IList<ConversationLog> Logs { get; set; }

        public virtual SocialUser LastMessageSender { get; set; }
    }
}
