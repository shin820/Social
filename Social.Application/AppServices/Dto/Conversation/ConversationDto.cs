using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class ConversationDto
    {
        public int Id { get; set; }

        public ConversationSource Source { get; set; }

        public string SocialId { get; set; }

        public bool IsRead { get; set; }

        public TimeSpan HandlingTime { get; set; }

        public string Requester { get; set; }

        public string Receiver { get; set; }

        public int? AgentAssignee { get; set; }

        public int? DepartmentAssignee { get; set; }

        public ConversationStatus Status { get; set; } = ConversationStatus.New;

        public string Subject { get; set; }

        public string Comment { get; set; }

        public ConversationPriority Priority { get; set; } = ConversationPriority.Normal;

        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }
}
