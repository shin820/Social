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
        Task ProcessTaggedData(SocialAccount socialAccount);
        void InsertWebHookData(string data);
    }

    public class FacebookAppService : AppService, IFacebookAppService
    {
        private IWebHookService _facebookWebHookService;
        private IRepository<FacebookWebHookRawData> _hookDataRepo;
        private ITaggedVisitorPostService _taggedVisitorPostService;

        public FacebookAppService(
            IWebHookService facebookWebHookService,
            ITaggedVisitorPostService taggedVisitorPostService,
            IRepository<FacebookWebHookRawData> hookDataRepo)
        {
            _facebookWebHookService = facebookWebHookService;
            _taggedVisitorPostService = taggedVisitorPostService;
            _hookDataRepo = hookDataRepo;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            await _facebookWebHookService.ProcessWebHookData(fbData);
        }

        public async Task ProcessTaggedData(SocialAccount socialAccount)
        {
            await _taggedVisitorPostService.Process(socialAccount);
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
