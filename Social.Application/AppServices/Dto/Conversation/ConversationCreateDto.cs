using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class ConversationCreateDto
    {
        [Required]
        public ConversationSource Source { get; set; }

        [Required]
        public string SocialId { get; set; }

        [Required]
        public string Requester { get; set; }

        [Required]
        public string Receiver { get; set; }

        public int? AgentAssignee { get; set; }

        public int? DepartmentAssignee { get; set; }

        [Required]
        [MaxLength(300)]
        public string Subject { get; set; }

        [Required]
        public ConversationPriority Priority { get; set; } = ConversationPriority.Normal;
    }
}
