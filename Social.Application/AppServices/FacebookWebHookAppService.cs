using Framework.Core;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.AppServices
{
    public interface IFacebookWebHookAppService
    {
        Task ProcessWebHookData(FbHookData fbData);
        void InsertWebHookData(string data);
    }

    public class FacebookWebHookAppService : AppService, IFacebookWebHookAppService
    {
        private IFacebookWebHookService _facebookWebHookService;
        private IRepository<FacebookWebHookRawData> _hookDataRepo;

        public FacebookWebHookAppService(IFacebookWebHookService facebookWebHookService, IRepository<FacebookWebHookRawData> hookDataRepo)
        {
            _facebookWebHookService = facebookWebHookService;
            _hookDataRepo = hookDataRepo;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            await _facebookWebHookService.ProcessWebHookData(fbData);
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
