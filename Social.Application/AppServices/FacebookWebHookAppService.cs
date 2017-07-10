using Framework.Core;
using Social.Domain.DomainServices;
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
    }

    public class FacebookWebHookAppService : AppService, IFacebookWebHookAppService
    {
        private IFacebookWebHookService _facebookWebHookService;

        public FacebookWebHookAppService(IFacebookWebHookService facebookWebHookService)
        {
            _facebookWebHookService = facebookWebHookService;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            await _facebookWebHookService.ProcessWebHookData(fbData);
        }
    }
}
