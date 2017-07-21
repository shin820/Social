﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class FacebookPostMessageDto
    {
        public int Id { get; set; }
        public string UserAvatar { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserScreenName { get; set; }
        public string Content { get; set; }
        public string OriginalLink { get; set; }
        public DateTime SendTime { get; set; }

        public List<FacebookPostCommentMessageDto> Comments { get; set; }
        public List<MessageAttachmentDto> Attachments { get; set; }
    }
}
