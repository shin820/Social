using Framework.Core;
using Social.Domain.DomainServices;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IFacebookAppService
    {
        Task ProcessWebHookData(FbHookData fbData);
        Task PullTaggedVisitorPosts(SocialAccount socialAccount);
        Task PullVisitorPostsFromFeed(SocialAccount socialAccount);
        void InsertWebHookData(string data);
    }

    public class FacebookAppService : AppService, IFacebookAppService
    {
        private IWebHookService _facebookWebHookService;
        private IRepository<FacebookWebHookRawData> _hookDataRepo;
        private IVisitorPostService _visitorPostService;

        public FacebookAppService(
            IWebHookService facebookWebHookService,
            IVisitorPostService visitorPostService,
            IRepository<FacebookWebHookRawData> hookDataRepo)
        {
            _facebookWebHookService = facebookWebHookService;
            _visitorPostService = visitorPostService;
            _hookDataRepo = hookDataRepo;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            await _facebookWebHookService.ProcessWebHookData(fbData);
        }

        public async Task PullTaggedVisitorPosts(SocialAccount socialAccount)
        {
            await _visitorPostService.PullTaggedVisitorPosts(socialAccount);
        }

        public async Task PullVisitorPostsFromFeed(SocialAccount socialAccount)
        {
            await _visitorPostService.PullVisitorPostsFromFeed(socialAccount);
        }

        public void InsertWebHookData(string data)
        {
            _hookDataRepo.Insert(new FacebookWebHookRawData
            {
                Data = data,
                CreatedTime = DateTime.UtcNow
            });
        }
    }
}
