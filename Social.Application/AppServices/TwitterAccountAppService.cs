using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace Social.Application.AppServices
{
    public interface ITwitterAccountAppService
    {
        Task AddAccountAsync(ITwitterCredentials credentials);
        IList<TwitterAccountListDto> GetAccounts();
        TwitterAccountDto GetAccount(int id);
        Task DeleteAccountAsync(int id);
        TwitterAccountDto UpdateAccount(int id, UpdateTwitterAccountDto dto);

    }

    public class TwitterAccountAppService : AppService, ITwitterAccountAppService
    {
        private ISocialAccountService _socialAccountService;

        public TwitterAccountAppService(ISocialAccountService socialAccountService)
        {
            _socialAccountService = socialAccountService;
        }

        public async Task AddAccountAsync(ITwitterCredentials credentials)
        {
            var user = User.GetAuthenticatedUser(credentials);
            SocialAccount account = new SocialAccount
            {
                Token = user.Credentials.AccessToken,
                TokenSecret = user.Credentials.AccessTokenSecret,
                SocialUser = new SocialUser
                {
                    Name = user.Name,
                    Type = SocialUserType.Twitter,
                    Avatar = user.ProfileImageUrl,
                    OriginalId = user.IdStr,
                    OriginalLink = user.Url
                }
            };

            await _socialAccountService.InsertAsync(account);
        }

        public IList<TwitterAccountListDto> GetAccounts()
        {
            return _socialAccountService.FindAll().Where(t => t.SocialUser.Type == SocialUserType.Twitter).ProjectTo<TwitterAccountListDto>().ToList();
        }

        public TwitterAccountDto GetAccount(int id)
        {
            var entity = _socialAccountService.Find(id);
            return Mapper.Map<TwitterAccountDto>(entity);
        }

        public async Task DeleteAccountAsync(int id)
        {
            var entity = _socialAccountService.Find(id);
            if (entity != null)
            {
                await _socialAccountService.DeleteAsync(entity);
            }
        }

        public TwitterAccountDto UpdateAccount(int id, UpdateTwitterAccountDto dto)
        {
            var socialAccount = _socialAccountService.Find(id);
            if (socialAccount == null)
            {
                throw new NotFoundException($"'{id}' not exists.");
            }

            socialAccount = Mapper.Map(dto, socialAccount);
            _socialAccountService.Update(socialAccount);

            return Mapper.Map<TwitterAccountDto>(socialAccount);
        }

    }
}
