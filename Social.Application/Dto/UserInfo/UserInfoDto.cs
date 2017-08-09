using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Application.Dto.UserInfo
{
    public class UserInfoDto
    {
        public List<Conversation> Conversations { get; set; }
        public FbUser FbUser { get; set; }
        public IUser TwitterUser { get; set; }
    }
}
