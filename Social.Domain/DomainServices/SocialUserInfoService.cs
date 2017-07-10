using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface ISocialUserInfoService
    {
        Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail);
    }

    public class SocialUserInfoService : DomainService<SocialUser>, ISocialUserInfoService
    {
        public async Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail)
        {
            var user = Repository.FindAll().Where(t => t.SiteId == siteId && t.SocialId == fbUserId).FirstOrDefault();
            if (user == null)
            {
                user = await FacebookService.GetUserInfo(token, fbUserId, fbUserEmail);
                user.SiteId = siteId;
                await Repository.InsertAsync(user);
            }

            //bool ifUserInfoUpdated =
            //    socialUser.Email != facebookUser.Email
            //    || socialUser.Avatar != facebookUser.Avatar;
            //if (ifUserInfoUpdated)
            //{
            //    socialUser.Email = facebookUser.Email;
            //    socialUser.Avatar = facebookUser.Avatar;
            //    await Repository.UpdateAsync(socialUser);
            //    return socialUser;
            //}

            return user;
        }
    }
}
