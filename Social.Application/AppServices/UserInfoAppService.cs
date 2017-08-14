using AutoMapper;
using Framework.Core;
using Social.Application.Dto;
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
        IList<ConversationDto> FindConversations(string OriginalId);
    }
    public class UserInfoAppService: AppService, IUserInfoAppService
    {
        private IUserInfoService _domainService;
        private IConversationAppService _conversationService;
        public UserInfoAppService(IUserInfoService domainService, IConversationAppService conversationService)
        {
            _domainService = domainService;
            _conversationService = conversationService;
        }


        public UserInfoDto Find(string OriginalId)
        {
            UserInfoDto userInfoDto = new UserInfoDto();
            SocialUser user = _domainService.GetUser(OriginalId);
            if(user!= null)
            {
                List<Conversation> conversations = _domainService.GetConversations(user.Id);
                if (user.Source == Infrastructure.Enum.SocialUserSource.Facebook && conversations.Count >0)
                {
                    FbUser facebookInfo = _domainService.GetFacebookInfo(OriginalId, conversations.First(), user.Id);
                    userInfoDto = Mapper.Map<UserInfoDto>(facebookInfo);
                }
                else if(user.Source == Infrastructure.Enum.SocialUserSource.Twitter && conversations.Count > 0)
                {
                    IUser twitterUserInfo = _domainService.GetTwitterInfo(OriginalId, conversations.First(), user.Id);
                    userInfoDto.name = twitterUserInfo.Name;
                    userInfoDto.id = twitterUserInfo.Id.ToString();
                    userInfoDto.pic = twitterUserInfo.ProfileImageUrl;
                    userInfoDto.ScreenName = twitterUserInfo.ScreenName;
                    userInfoDto.Location = twitterUserInfo.Location;
                    userInfoDto.FollowersCount = twitterUserInfo.FollowersCount;
                    userInfoDto.FriendsCount = twitterUserInfo.FriendsCount;
                    userInfoDto.StatusesCount = twitterUserInfo.StatusesCount;
                    userInfoDto.Description = twitterUserInfo.Description;
                    userInfoDto.JoinedDate = twitterUserInfo.CreatedAt.ToString();
                    if (twitterUserInfo.Entities.Website != null)
                    {
                        userInfoDto.Website = twitterUserInfo.Entities.Website.Urls.First().DisplayedURL;
                    }
                }
            }
            return userInfoDto;
        }

        public IList<ConversationDto> FindConversations(string OriginalId)
        {
            SocialUser user = _domainService.GetUser(OriginalId);
            ConversationSearchDto searchDto = new ConversationSearchDto();
            if (user != null)
            {
                searchDto.UserId = user.Id;
                return _conversationService.Find(searchDto);
            }
            return null;
        }
    }
}
