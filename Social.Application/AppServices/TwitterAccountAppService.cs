using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Credentials.Models;
using Tweetinvi.Models;

namespace Social.Application.AppServices
{
    public interface ITwitterAccountAppService
    {
        IAuthenticationContext InitAuthentication(string redirectUri);
        Task AddAccountAsync(string authorizationId, string oauthVerifier);
        IList<TwitterAccountListDto> GetAccounts();
        TwitterAccountDto GetAccount(int id);
        Task DeleteAccountAsync(int id);
        TwitterAccountDto UpdateAccount(int id, UpdateTwitterAccountDto dto);
        TwitterAccountDto MarkAsEnable(int id, bool? ifEnable = true);

    }

    public class TwitterAccountAppService : AppService, ITwitterAccountAppService
    {
        private ISocialAccountService _socialAccountService;
        private ISocialUserService _socialUserService;
        private ITwitterAuthService _twitterAuthService;

        public TwitterAccountAppService(
            ISocialAccountService socialAccountService,
            ISocialUserService socialUserService,
            ITwitterAuthService twitterAuthService
            )
        {
            _socialAccountService = socialAccountService;
            _socialUserService = socialUserService;
            _twitterAuthService = twitterAuthService;
        }


        public IAuthenticationContext InitAuthentication(string redirectUri)
        {
            return _twitterAuthService.InitAuthentication(redirectUri);
        }

        public async Task AddAccountAsync(string authorizationId, string oauthVerifier)
        {
            var user = await _twitterAuthService.ValidateAuthAsync(authorizationId, oauthVerifier);
            if (user != null)
            {
                SocialAccount account = new SocialAccount
                {
                    Token = user.Credentials.AccessToken,
                    TokenSecret = user.Credentials.AccessTokenSecret,
                    IfConvertMessageToConversation = true,
                    IfConvertTweetToConversation = true,
                    IfEnable = true
                };

                var socialUser = _socialUserService.FindByOriginalId(user.IdStr, SocialUserSource.Twitter, SocialUserType.Customer);
                if (socialUser != null)
                {
                    //convert customer to integration account;
                    socialUser.Type = SocialUserType.IntegrationAccount;
                    socialUser.SocialAccount = account;
                    _socialUserService.Update(socialUser);
                    account.SocialUser = socialUser;
                    await _socialAccountService.InsertSocialAccountInGeneralDb(account);
                }
                else
                {
                    account.SocialUser = new SocialUser
                    {
                        Name = user.Name,
                        ScreenName = user.ScreenName,
                        Email = user.Email,
                        Source = SocialUserSource.Twitter,
                        Type = SocialUserType.IntegrationAccount,
                        Avatar = user.ProfileImageUrl,
                        OriginalId = user.IdStr,
                        OriginalLink = TwitterHelper.GetUserUrl(user.ScreenName)
                    };

                    await _socialAccountService.InsertAsync(account);
                }
            }
        }

        public IList<TwitterAccountListDto> GetAccounts()
        {
            return _socialAccountService.FindAll().Where(t => t.SocialUser.Source == SocialUserSource.Twitter).ProjectTo<TwitterAccountListDto>().ToList();
        }

        public TwitterAccountDto GetAccount(int id)
        {
            var entity = _socialAccountService.FindAccount(id, SocialUserSource.Twitter);
            if (entity == null)
            {
                throw SocialExceptions.TwitterAccountNotExists(id);
            }

            return Mapper.Map<TwitterAccountDto>(entity);
        }

        public async Task DeleteAccountAsync(int id)
        {
            var entity = _socialAccountService.FindAccount(id, SocialUserSource.Twitter);
            if (entity == null)
            {
                throw SocialExceptions.TwitterAccountNotExists(id);
            }

            await _socialAccountService.DeleteAsync(entity);
        }

        public TwitterAccountDto UpdateAccount(int id, UpdateTwitterAccountDto dto)
        {
            var socialAccount = _socialAccountService.FindAccount(id, SocialUserSource.Twitter);
            if (socialAccount == null)
            {
                throw SocialExceptions.TwitterAccountNotExists(id);
            }

            socialAccount = Mapper.Map(dto, socialAccount);
            _socialAccountService.Update(socialAccount);

            return Mapper.Map<TwitterAccountDto>(socialAccount);
        }

        public TwitterAccountDto MarkAsEnable(int id, bool? ifEnable = true)
        {
            var socialAccount = _socialAccountService.MarkAsEnable(id, ifEnable);
            return Mapper.Map<TwitterAccountDto>(socialAccount);
        }
    }
}
