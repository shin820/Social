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
        public string SocialId { get; set; }

        [Required]
        public SocialUserType Type { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }

        public string Avatar { get; set; }

        [MaxLength(200)]
        public string SocialLink { get; set; }

        public int? SocialAccountId { get; set; }

        [NotMapped]
        public bool IsSocialAccount
        {
            get
            {
                return SocialAccountId != null;
            }
        }

        public virtual SocialAccount SocialAccount { get; set; }

        public virtual IList<Message> SendMessages { get; set; }
        public virtual IList<Message> ReceiveMessages { get; set; }
        public virtual IList<Conversation> LastSendConversations { get; set; }
    }
}
