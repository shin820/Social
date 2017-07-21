using AutoMapper;
using AutoMapper.QueryableExtensions;
using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IFacebookAccountAppService
    {
        FacebookPageDto GetPage(int id);
        Task<PendingAddFacebookPagesDto> GetPendingAddPagesAsync(string code, string redirectUri);
        Task<FacebookPageDto> AddPageAsync(AddFaceboookPageDto dto);
        Task DeletePageAsync(int id);
        IList<FacebookPageListDto> GetPages();
        FacebookPageDto UpdatePage(int id, UpdateFacebookPageDto dto);
    }

    public class FacebookAccountAppService : AppService, IFacebookAccountAppService
    {
        private ISocialAccountService _socialAccountService;

        public FacebookAccountAppService(ISocialAccountService socialAccountService)
        {
            _socialAccountService = socialAccountService;
        }

        public IList<FacebookPageListDto> GetPages()
        {
            return _socialAccountService.FindAll().Where(t => t.SocialUser.Source == SocialUserSource.Facebook).ProjectTo<FacebookPageListDto>().ToList();
        }

        public async Task<PendingAddFacebookPagesDto> GetPendingAddPagesAsync(string code, string redirectUri)
        {
            string userToken = FbClient.GetUserToken(code, redirectUri);
            FbUser me = await FbClient.GetMe(userToken);
            IList<FbPage> pages = await FbClient.GetPages(userToken);
            List<string> pageIds = pages.Select(t => t.Id).ToList();
            var facebookAccounts = _socialAccountService.FindAll()
                .Where(t => t.SocialUser.Source == SocialUserSource.Facebook && pageIds.Contains(t.SocialUser.OriginalId))
                .ToList();

            var result = new PendingAddFacebookPagesDto
            {
                SignInAs = new FacebookSignInAsDto
                {
                    Name = me.name,
                    Avatar = me.pic
                },
                Pages = pages.Select(t => new PendingAddFacebookPageDto
                {
                    FacebookId = t.Id,
                    Name = t.Name,
                    Category = t.Category,
                    Avatar = t.Avatar,
                    AccessToken = t.AccessToken,
                    Link = t.Link,
                    IsAdded = facebookAccounts.Any(m => m.SocialUser.OriginalId == t.Id)
                }).ToList()
            };

            return result;
        }

        public FacebookPageDto GetPage(int id)
        {
            var entity = _socialAccountService.Find(id);
            return Mapper.Map<FacebookPageDto>(entity);
        }

        public async Task<FacebookPageDto> AddPageAsync(AddFaceboookPageDto dto)
        {
            var socialAccount = Mapper.Map<SocialAccount>(dto);
            socialAccount.SocialUser = Mapper.Map<SocialUser>(dto);

            await _socialAccountService.InsertAsync(socialAccount);
            await FbClient.SubscribeApp(dto.FacebookId, dto.AccessToken);

            socialAccount = _socialAccountService.Find(socialAccount.Id);
            return Mapper.Map<FacebookPageDto>(socialAccount);
        }

        public async Task DeletePageAsync(int id)
        {
            var entity = _socialAccountService.Find(id);
            if (entity != null)
            {
                await _socialAccountService.DeleteAsync(entity);
                await FbClient.UnSubscribeApp(entity.SocialUser.OriginalId, entity.Token);
            }
        }

        public FacebookPageDto UpdatePage(int id, UpdateFacebookPageDto dto)
        {
            var socialAccount = _socialAccountService.Find(id);
            if (socialAccount == null)
            {
                throw new NotFoundException($"'{id}' not exists.");
            }

            socialAccount = Mapper.Map(dto, socialAccount);
            _socialAccountService.Update(socialAccount);

            return Mapper.Map<FacebookPageDto>(socialAccount);
        }
    }
}
