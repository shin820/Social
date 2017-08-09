using Social.Application.Dto.UserInfo;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Application.AppServices
{
    public interface IUserInfoAppService
    {
        UserInfoDto Find(string OriginalId);
    }
    public class UserInfoAppService:IUserInfoAppService
    {
        private IUserInfoService _domainService;
        public UserInfoAppService(IUserInfoService domainService)
        {
            _domainService = domainService;
        }


        public UserInfoDto Find(string OriginalId)
        {
            UserInfoDto userInfoDto = new UserInfoDto();
            SocialUser user = _domainService.GetUser(OriginalId);
            if(user!= null)
            {
                List<Conversation> conversations = _domainService.GetConversations(user.Id);
                userInfoDto.Conversations = conversations.OrderBy(t => t.LastMessageSentTime).ToList();
                if (user.Source == Infrastructure.Enum.SocialUserSource.Facebook)
                {
                    FbUser facebookInfo = _domainService.GetFacebookInfo(OriginalId, conversations.First(), user.Id);
                    userInfoDto.FbUser = facebookInfo;
                }
                else if(user.Source == Infrastructure.Enum.SocialUserSource.Twitter)
                {
                    IUser twitterUserInfo = _domainService.GetTwitterInfo(OriginalId, conversations.First(), user.Id);
                    userInfoDto.TwitterUser = twitterUserInfo;
                }
            }
            return userInfoDto;
        }
    }
}
