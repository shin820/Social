using Facebook;
using Framework.Core;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Social.Domain.DomainServices.Facebook
{
    public interface IWebHookService : ITransient
    {
        Task ProcessWebHookData(FbHookData fbData);
    }

    public class WebHookService : ServiceBase, IWebHookService
    {
        private ISocialAccountService _socialAccountService;
        private IConversationStrategyFactory _strategyFactory;
        private IRepository<GeneralDataContext, SiteSocialAccount> _siteSocialAccountRepo;

        public WebHookService(
            ISocialAccountService socialAccountService,
            IConversationStrategyFactory strategyFactory,
            IRepository<GeneralDataContext, SiteSocialAccount> siteSocialAccountRepo
            )
        {
            _socialAccountService = socialAccountService;
            _strategyFactory = strategyFactory;
            _siteSocialAccountRepo = siteSocialAccountRepo;
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

            SocialAccount socialAccount = await GetSoicalAccount(pageId);
            if (socialAccount == null)
            {
                return;
            }

            foreach (var change in changes)
            {
                if (socialAccount != null)
                {
                    var strategory = _strategyFactory.Create(change);
                    if (strategory != null)
                    {
                        await UnitOfWorkManager.Run(TransactionScopeOption.RequiresNew, socialAccount.SiteId, async () =>
                         {
                             await strategory.Process(socialAccount, change);
                         });
                    }
                }
            }
        }

        private async Task<SocialAccount> GetSoicalAccount(string pageId)
        {
            SiteSocialAccount siteSocialAccount = null;
            await UnitOfWorkManager.RunWithoutTransaction(null, async () =>
            {
                siteSocialAccount = await _siteSocialAccountRepo.FindAll().Where(t => t.FacebookPageId == pageId).FirstOrDefaultAsync();
            });
            if (siteSocialAccount == null)
            {
                return null;
            }

            SocialAccount socialAccount = null;
            await UnitOfWorkManager.RunWithoutTransaction(siteSocialAccount.SiteId, async () =>
            {
                socialAccount = await _socialAccountService.GetAccountAsync(SocialUserSource.Facebook, pageId);
            });
            if (socialAccount == null)
            {
                return null;
            }

            return socialAccount;
        }
    }
}
