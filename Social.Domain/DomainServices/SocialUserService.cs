using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices
{
    public interface ISocialUserService : IDomainService<SocialUser>
    {
        //Task<SocialUser> GetOrCreateSocialUser(int siteId, string token, string fbUserId, string fbUserEmail);
        Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser);
        Task<SocialUser> GetOrCreateTwitterUser(string orignalUserId);
        Task<SocialUser> GetOrCreateFacebookUser(string token, string fbUserId);
        Task<List<SocialUser>> GetOrCreateSocialUsers(string token, List<FbUser> fbSenders);
        SocialUser FindByOriginalId(string originalId, SocialUserSource souce, SocialUserType type);
    }

    public class SocialUserService : DomainService<SocialUser>, ISocialUserService
    {
        private IFbClient _fbClient;

        public SocialUserService(IFbClient fbClient)
        {
            _fbClient = fbClient;
        }


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
            return base.FindAll().Where(t => t.IsDeleted == false).FirstOrDefault(t => t.Id == id);
        }

        public SocialUser FindByOriginalId(string originalId, SocialUserSource source, SocialUserType type)
        {
            return FindAll().Where(t => t.OriginalId == originalId && t.Source == source && t.Type == type).FirstOrDefault();
        }

        public SocialUser FindByOriginalId(string originalId, SocialUserSource source)
        {
            return FindAll().Where(t => t.OriginalId == originalId && t.Source == source).FirstOrDefault();
        }

        public async Task<SocialUser> GetOrCreateTwitterUser(IUser twitterUser)
        {
            var user = FindByOriginalId(twitterUser.IdStr, SocialUserSource.Twitter);
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
                    OriginalLink = TwitterHelper.GetUserUrl(twitterUser.ScreenName)
                };

                await UnitOfWorkManager.RunWithNewTransaction(CurrentUnitOfWork.GetSiteId(), async () =>
                {
                    await Repository.InsertAsync(user);
                    CurrentUnitOfWork.SaveChanges();
                });
            }
            return user;
        }

        public async Task<SocialUser> GetOrCreateTwitterUser(string orignalUserId)
        {
            var user = FindByOriginalId(orignalUserId, SocialUserSource.Twitter);
            if (user == null)
            {
                IUser twitterUser = User.GetUserFromId(long.Parse(orignalUserId));
                user = FindByOriginalId(orignalUserId, SocialUserSource.Twitter);
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
                        OriginalLink = TwitterHelper.GetUserUrl(twitterUser.ScreenName)
                    };

                    await UnitOfWorkManager.RunWithNewTransaction(CurrentUnitOfWork.GetSiteId(), async () =>
                    {
                        await Repository.InsertAsync(user);
                        CurrentUnitOfWork.SaveChanges();
                    });
                }
            }
            return user;
        }

        public async Task<SocialUser> GetOrCreateFacebookUser(string token, string fbUserId)
        {
            var user = FindByOriginalId(fbUserId, SocialUserSource.Facebook);
            if (user == null)
            {
                FbUser fbUser = await _fbClient.GetUserInfo(token, fbUserId);
                user = FindByOriginalId(fbUserId, SocialUserSource.Facebook);
                if (user == null)
                {
                    user = new SocialUser
                    {
                        OriginalId = fbUser.id,
                        Name = fbUser.name,
                        Email = fbUser.email,
                        OriginalLink = fbUser.link,
                        Avatar = fbUser.pic,
                        Source = SocialUserSource.Facebook
                    };

                    await UnitOfWorkManager.RunWithNewTransaction(CurrentUnitOfWork.GetSiteId(), async () =>
                    {
                        await InsertAsync(user);
                        CurrentUnitOfWork.SaveChanges();
                    });
                }
            }
            return user;
        }

        public async Task<List<SocialUser>> GetOrCreateSocialUsers(string token, List<FbUser> fbSenders)
        {
            var uniqueFbSenders = new List<FbUser>();
            foreach (var fbSender in fbSenders)
            {
                if (!uniqueFbSenders.Any(t => t.id == fbSender.id))
                {
                    uniqueFbSenders.Add(fbSender);
                }
            }

            List<SocialUser> senders = new List<SocialUser>();
            var fbSenderIds = uniqueFbSenders.Select(t => t.id).ToList();
            var existingUsers = FindAll().Where(t => t.Source == SocialUserSource.Facebook && fbSenderIds.Contains(t.OriginalId)).ToList();
            senders.AddRange(existingUsers);
            uniqueFbSenders.RemoveAll(t => existingUsers.Any(e => e.OriginalId == t.id));
            foreach (var fbSender in uniqueFbSenders)
            {
                var sender = new SocialUser()
                {
                    OriginalId = fbSender.id,
                    Name = fbSender.name,
                    Email = fbSender.email,
                    Source = SocialUserSource.Facebook,
                };

                await UnitOfWorkManager.RunWithNewTransaction(CurrentUnitOfWork.GetSiteId(), async () =>
                {
                    await InsertAsync(sender);
                    senders.Add(sender);
                });
            }
            return senders;
        }
    }
}
