using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class MessageAttachmentDto
    {
        public int Id { get; set; }
        public MessageAttachmentType Type { get; set; }
        public string Url { get; set; }
        public string PreviewUrl { get; set; }
    }
}
