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
    public interface ISocialUserService : IDomainService<SocialUser>
    {
        //Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail);
        Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser);
        SocialUser Get(string originalId, SocialUserSource souce, SocialUserType type);
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

        public override IQueryable<SocialUser> FindAll()
        {
            return base.FindAll().Where(t => t.IsDeleted == false);
        }

        public override SocialUser Find(int id)
        {
            return base.FindAll().Where(t => t.IsDeleted == false).FirstOrDefault();
        }

        public SocialUser Get(string originalId, SocialUserSource source, SocialUserType type)
        {
            return Repository.FindAll().Where(t => t.OriginalId == originalId && t.IsDeleted == false && t.Source == source && t.Type == type).FirstOrDefault();
        }

        public async Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser)
        {
            var user = Repository.FindAll()
                .Where(t => t.OriginalId == twitterUser.IdStr && t.Source == SocialUserSource.Twitter && t.IsDeleted == false)
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
                    Type = SocialUserType.Customer,
                    OriginalLink = twitterUser.Url
                };
                await Repository.InsertAsync(user);
            }
            return user;
        }
    }
}
