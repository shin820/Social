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
    [Table("t_Social_Message")]
    public class Message : EntityWithSite
    {
        public Message()
        {
            Attachments = new List<MessageAttachment>();
            Children = new List<Message>();
        }

        public int ConversationId { get; set; }

        public MessageSource Source { get; set; }

        [Required]
        public string SocialId { get; set; }

        [MaxLength(500)]
        public string SocialLink { get; set; }

        public int? ParentId { get; set; }

        public DateTime SendTime { get; set; }

        public int SenderId { get; set; }

        [NotMapped]
        public string SenderSocialId { get; set; }
        [NotMapped]
        public string SenderEmail { get; set; }

        public int ReceiverId { get; set; }

        public string Content { get; set; }

        public virtual Conversation Conversation { get; set; }

        public virtual IList<MessageAttachment> Attachments { get; set; }

        public virtual Message Parent { get; set; }

        public virtual SocialUser Sender { get; set; }

        public virtual SocialUser Receiver { get; set; }

        public virtual IList<Message> Children { get; set; }
    }
}
