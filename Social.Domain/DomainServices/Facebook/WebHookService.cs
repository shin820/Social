using Facebook;
using Framework.Core;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Facebook
{
    public interface IWebHookService : ITransient
    {
        Task ProcessWebHookData(FbHookData fbData);
    }

    public class WebHookService : ServiceBase, IWebHookService
    {
        private IRepository<SocialAccount> _socialAccountRepo;
        private IConversationStrategyFactory _strategyFactory;

        public WebHookService(
            IRepository<SocialAccount> socialAccountRepo,
            IConversationStrategyFactory strategyFactory
            )
        {
            _socialAccountRepo = socialAccountRepo;
            _strategyFactory = strategyFactory;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            if (fbData == null || !fbData.Entry.Any())
            {
                return;
            }

            var changes = fbData.Entry.First().Changes;
            if (changes == null || !changes.Any())
            {
                return;
            }

            if (fbData.Object != "page")
            {
                return;
            }

            string pageId = fbData.Entry.First().Id;
            var socialAccount = _socialAccountRepo.FindAll().FirstOrDefault(t => t.SocialUser.SocialId == pageId);
            foreach (var change in changes)
            {
                if (socialAccount != null)
                {
                    var strategory = _strategyFactory.Create(change);
                    if (strategory != null)
                    {
                        await strategory.Process(socialAccount, change);
                    }
                }
            }
        }
    }
}
