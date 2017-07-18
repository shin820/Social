using Framework.Core;
using Social.Application.Dto;
using Social.Domain.DomainServices;
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
    public interface ISocialAccountAppService
    {
        Task<FacebookPagesToBeAddDto> GetFacebookPages(string code, string redirectUri);
    }

    public class SocialAccountAppService : AppService, ISocialAccountAppService
    {
        private ISocialAccountService _socialAccountService;

        public SocialAccountAppService(ISocialAccountService socialAccountService)
        {
            _socialAccountService = socialAccountService;
        }

        public async Task<FacebookPagesToBeAddDto> GetFacebookPages(string code, string redirectUri)
        {
            string userToken = FbClient.GetUserToken(code, redirectUri);
            FbUser me = await FbClient.GetMe(userToken);
            IList<FbPage> pages = await FbClient.GetPages(userToken);
            var facebookAccounts = _socialAccountService.FindAll().Include(t => t.SocialUser).Where(t => t.SocialUser.Type == SocialUserType.Facebook).ToList();

            var result = new FacebookPagesToBeAddDto
            {
                SignInAs = new FacebookSignInAsDto
                {
                    Name = me.name,
                    Avatar = me.pic
                },
                Pages = pages.Select(t => new FacebookPageToBeAddDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Category = t.Category,
                    Avatar = t.Avatar,
                    AccessToken = t.AccessToken,
                    IsAdded = facebookAccounts.Any(m => m.SocialUser.OriginalId == t.Id)
                }).ToList()
            };

            return result;
        }
    }
}
