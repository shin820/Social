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
    [Table("t_Social_Account")]
    public class SocialAccount : EntityWithSite, IHaveCreation, IHaveModification
    {
        public SocialAccount()
        {
            Conversations = new List<Conversation>();
        }

        [Required]
        public string Token { get; set; }

        public bool IfEnable { get; set; }

        public bool IfConvertMessageToConversation { get; set; }

        public bool IfConvertVisitorPostToConversation { get; set; }

        public bool IfConvertWallPostToConversation { get; set; }

        public bool IfConvertTweetToConversation { get; set; }

        public string FacebookPageCategory { get; set; }

        public string FacebookSignInAs { get; set; }

        public DateTime CreatedTime { get; set; }

        public int CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public int? ConversationDepartmentId { get; set; }

        public int? ConversationAgentId { get; set; }

        public ConversationStatus Status { get; set; }

        public ConversationPriority? ConversationPriority { get; set; }

        public virtual IList<Conversation> Conversations { get; set; }

        [Required]
        public int SocialUserId { get; set; }
        public virtual SocialUser SocialUser { get; set; }
    }
}
