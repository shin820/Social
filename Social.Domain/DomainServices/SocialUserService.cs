using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices
{
    public interface ISocialUserService
    {
        //Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail);
        Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser);
    }

    public class SocialUserService : DomainService<SocialUser>, ISocialUserService
    {
        //public async Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail)
        //{
        //    var user = Repository.FindAll().Where(t => t.SiteId == siteId && t.OriginalId == fbUserId).FirstOrDefault();
        //    if (user == null)
        //    {
        //        FbUser fbUser = await FbClient.GetUserInfo(token, fbUserId);
        //        user = new SocialUser
        //        {
        //            OriginalId = fbUser.id,
        //            Name = fbUser.name,
        //            Email = fbUser.email
        //        };
        //        user.SiteId = siteId;
        //        await Repository.InsertAsync(user);
        //    }

        //    //bool ifUserInfoUpdated =
        //    //    socialUser.Email != facebookUser.Email
        //    //    || socialUser.Avatar != facebookUser.Avatar;
        //    //if (ifUserInfoUpdated)
        //    //{
        //    //    socialUser.Email = facebookUser.Email;
        //    //    socialUser.Avatar = facebookUser.Avatar;
        //    //    await Repository.UpdateAsync(socialUser);
        //    //    return socialUser;
        //    //}

        //    return user;
        //}

        public async Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser)
        {
            var user = Repository.FindAll()
                .Where(t => t.OriginalId == twitterUser.IdStr && t.Source == SocialUserSource.Twitter)
                .FirstOrDefault();
            if (user == null)
            {
                user = new SocialUser
                {
                    OriginalId = twitterUser.IdStr,
                    Name = twitterUser.Name,
                    ScreenName = twitterUser.ScreenName,
                    Avatar = twitterUser.ProfileImageUrl,
                    Source = SocialUserSource.Twitter,
                    OriginalLink = twitterUser.Url
                };
                await Repository.InsertAsync(user);
            }
            return user;
        }
    }
}
