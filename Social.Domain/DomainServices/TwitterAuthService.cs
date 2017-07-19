using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Credentials.Models;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices
{
    public interface ITwitterAuthService : IDomainService<TwitterAuth>
    {
        IAuthenticationContext InitAuthentication(string redirectUri);
        Task<IAuthenticatedUser> ValidateAuthAsync(string authorizationId, string oauthVerifier);
    }

    public class TwitterAuthService : DomainService<TwitterAuth>, ITwitterAuthService
    {
        private async Task<TwitterAuth> FindAndDeleteAsync(string authorizationId)
        {
            var auth = Repository.FindAll().Where(t => t.AuthorizationId == authorizationId).FirstOrDefault();

            if (auth != null)
            {
                await UnitOfWorkManager.RunWithNewTransaction(CurrentUnitOfWork.GetSiteId(), async () =>
                 {
                     await Repository.DeleteAsync(auth.Id);
                 });
            }

            return auth;
        }

        public IAuthenticationContext InitAuthentication(string redirectUri)
        {
            var appCreds = new ConsumerCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret);
            IAuthenticationContext authenticationContext = AuthFlow.InitAuthentication(appCreds, redirectUri);

            Repository.Insert(new TwitterAuth
            {
                AuthorizationId = authenticationContext.Token.AuthorizationUniqueIdentifier,
                AuthorizationKey = authenticationContext.Token.AuthorizationKey,
                AuthorizationSecret = authenticationContext.Token.AuthorizationSecret
            });

            return authenticationContext;
        }

        public async Task<IAuthenticatedUser> ValidateAuthAsync(string authorizationId, string oauthVerifier)
        {
            var twitterAuth = await FindAndDeleteAsync(authorizationId);

            var appCreds = new ConsumerCredentials(AppSettings.TwitterConsumerKey, AppSettings.TwitterConsumerSecret);
            var token = new AuthenticationToken()
            {
                AuthorizationKey = twitterAuth.AuthorizationKey,
                AuthorizationSecret = twitterAuth.AuthorizationSecret,
                ConsumerCredentials = appCreds
            };

            var userCredentils = AuthFlow.CreateCredentialsFromVerifierCode(oauthVerifier, token);
            var user = User.GetAuthenticatedUser(userCredentils);
            return user;
        }
    }
}
