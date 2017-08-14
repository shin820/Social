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
        //facebook
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string pic { get; set; }
        public string link { get; set; }


        //Twitter
        public string Website { get; set; }
        public string ScreenName { get; set; }
        public string JoinedDate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int? FollowersCount { get; set; }
        public int? FriendsCount { get; set; }
        public int? StatusesCount { get; set; }
    }
}
