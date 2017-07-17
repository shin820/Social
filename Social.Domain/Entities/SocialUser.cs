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
    [Table("t_Social_User")]
    public class SocialUser : EntityWithSite
    {
        public SocialUser()
        {
            SendMessages = new List<Message>();
            ReceiveMessages = new List<Message>();
            LastSendConversations = new List<Conversation>();
        }

        [Required]
        [MaxLength(200)]
        public string OriginalId { get; set; }

        [MaxLength(500)]
        public string OriginalLink { get; set; }

        [Required]
        public SocialUserType Type { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Email { get; set; }
        [MaxLength(200)]
        public string Avatar { get; set; }

        [NotMapped]
        public bool IsSocialAccount
        {
            get
            {
                return SocialAccount != null;
            }
        }

        public virtual SocialAccount SocialAccount { get; set; }

        public virtual IList<Message> SendMessages { get; set; }
        public virtual IList<Message> ReceiveMessages { get; set; }
        public virtual IList<Conversation> LastSendConversations { get; set; }
    }
}
