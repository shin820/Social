using Framework.Core;
using Social.Domain.Entities;
using Social.Domain.Repositories;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Social.Domain.DomainServices
{
    public interface ISocialAccountService : IDomainService<SocialAccount>
    {
        SocialAccount FindAccount(int id, SocialUserSource source);
        Task<SocialAccount> GetAccountAsync(SocialUserSource source, string originalId);
        Task<IList<SocialAccount>> GetAccountsAsync(SocialUserSource source);
        IQueryable<SocialAccount> FindAllTwitterAccounts();
        IQueryable<SocialAccount> FindAllFacebookAccounts();
        Task InsertSocialAccountInGeneralDb(SocialAccount entity);
        SocialAccount MarkAsEnable(int id, bool? ifEnable = true);
    }

    public class SocialAccountService : DomainService<SocialAccount>, ISocialAccountService
    {
        ISiteSocialAccountRepository _siteSocialAccountRepo;
        IRepository<SocialUser> _socialUserRepo;

        public SocialAccountService(
            ISiteSocialAccountRepository siteSocialAccountRepo,
            IRepository<SocialUser> socialUserRepo
            )
        {
            _siteSocialAccountRepo = siteSocialAccountRepo;
            _socialUserRepo = socialUserRepo;
        }

        public SocialAccount MarkAsEnable(int id, bool? ifEnable = true)
        {
            var account = this.Find(id);
            if (account == null)
            {
                throw SocialExceptions.SocialUserIdNotExists(id);
            }
            account.IfEnable = ifEnable.Value;
            this.Update(account);
            return account;
        }

        public override IQueryable<SocialAccount> FindAll()
        {
            return Repository.FindAll().Include(t => t.SocialUser).Where(t => t.IsDeleted == false);
        }

        public override SocialAccount Find(int id)
        {
            return Repository.FindAll().Include(t => t.SocialUser).Where(t => t.IsDeleted == false).FirstOrDefault(t => t.Id == id);
        }

        public IQueryable<SocialAccount> FindAllTwitterAccounts()
        {
            return FindAll().Where(t => t.SocialUser.Source == SocialUserSource.Twitter);
        }

        public IQueryable<SocialAccount> FindAllFacebookAccounts()
        {
            return FindAll().Where(t => t.SocialUser.Source == SocialUserSource.Facebook);
        }

        public SocialAccount FindAccount(int id, SocialUserSource source)
        {
            return Repository.FindAll().Include(t => t.SocialUser).Where(t => t.Id == id && t.IsDeleted == false && t.SocialUser.Source == source).FirstOrDefault();
        }

        public async Task<SocialAccount> GetAccountAsync(SocialUserSource source, string originalId)
        {
            return await Repository.FindAll().Include(t => t.SocialUser).Where(t => t.SocialUser.OriginalId == originalId && t.SocialUser.Source == source && t.IfEnable && t.IsDeleted == false).FirstOrDefaultAsync();
        }

        public async Task<IList<SocialAccount>> GetAccountsAsync(SocialUserSource source)
        {
            return await Repository.FindAll().Include(t => t.SocialUser).Where(t => t.SocialUser.Source == source && t.IfEnable && t.IsDeleted == false).ToListAsync();
        }

        public async override Task<SocialAccount> InsertAsync(SocialAccount entity)
        {
            if (IsDupliated(entity))
            {
                throw SocialExceptions.BadRequest($"'{entity.SocialUser.Name}' has already been added.");
            }

            ApplyDefaultValue(entity);
            base.Insert(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
            await InsertSocialAccountInGeneralDb(entity);

            return entity;
        }

        public async Task InsertSocialAccountInGeneralDb(SocialAccount entity)
        {
            int? siteIdOrNull = CurrentUnitOfWork.GetSiteId();
            if (siteIdOrNull == null)
            {
                throw new InvalidOperationException("site id must have value.");
            }

            int siteId = siteIdOrNull.Value;
            if (entity.SocialUser.Source == SocialUserSource.Facebook)
            {
                await UnitOfWorkManager.RunWithNewTransaction(null, async () =>
                {
                    if (!_siteSocialAccountRepo.FindAll().Any(t => t.SiteId == siteId && t.FacebookPageId == entity.SocialUser.OriginalId))
                    {
                        await _siteSocialAccountRepo.InsertAsync(new SiteSocialAccount { SiteId = siteId, FacebookPageId = entity.SocialUser.OriginalId });
                    }
                });
            }

            if (entity.SocialUser.Source == SocialUserSource.Twitter)
            {
                await UnitOfWorkManager.RunWithNewTransaction(null, async () =>
                {
                    if (!_siteSocialAccountRepo.FindAll().Any(t => t.SiteId == siteId && t.TwitterUserId == entity.SocialUser.OriginalId))
                    {
                        await _siteSocialAccountRepo.InsertAsync(new SiteSocialAccount { SiteId = siteId, TwitterUserId = entity.SocialUser.OriginalId });
                    }
                });
            }
        }

        public async override Task DeleteAsync(SocialAccount entity)
        {
            await base.DeleteAsync(entity);

            var user = _socialUserRepo.Find(entity.Id);
            _socialUserRepo.Delete(user);

            await DeleteSocialAccountInGeneralDb(entity);
        }

        private async Task DeleteSocialAccountInGeneralDb(SocialAccount entity)
        {
            int? siteIdOrNull = CurrentUnitOfWork.GetSiteId();
            if (siteIdOrNull == null)
            {
                throw new InvalidOperationException("site id must have value.");
            }

            int siteId = siteIdOrNull.Value;
            if (entity.SocialUser.Source == SocialUserSource.Facebook)
            {
                await UnitOfWorkManager.RunWithNewTransaction(null, async () =>
               {
                   var accoutns = _siteSocialAccountRepo.FindAll().Where(t => t.SiteId == siteId && t.FacebookPageId == entity.SocialUser.OriginalId).ToList();
                   foreach (var account in accoutns)
                   {
                       await _siteSocialAccountRepo.DeleteAsync(account);
                   }
               });
            }

            if (entity.SocialUser.Source == SocialUserSource.Twitter)
            {
                await UnitOfWorkManager.Run(TransactionScopeOption.Required, null, async () =>
                {
                    var accoutns = _siteSocialAccountRepo.FindAll().Where(t => t.SiteId == siteId && t.TwitterUserId == entity.SocialUser.OriginalId).ToList();
                    foreach (var account in accoutns)
                    {
                        await _siteSocialAccountRepo.DeleteAsync(account);
                    }
                });
            }
        }

        private void ApplyDefaultValue(SocialAccount entity)
        {
            entity.IfEnable = true;
            if (entity.SocialUser.Source == SocialUserSource.Facebook)
            {
                entity.IfConvertMessageToConversation = true;
                entity.IfConvertVisitorPostToConversation = true;
                entity.IfConvertWallPostToConversation = true;
            }
            if (entity.SocialUser.Source == SocialUserSource.Twitter)
            {
                entity.IfConvertMessageToConversation = true;
                entity.IfConvertTweetToConversation = true;
            }
        }

        private bool IsDupliated(SocialAccount entity)
        {
            if (entity.SocialUser.Source == SocialUserSource.Facebook)
            {
                return Repository.FindAll().Any(t => t.IsDeleted == false && t.SocialUser.OriginalId == entity.SocialUser.OriginalId && entity.SocialUser.Source == SocialUserSource.Facebook);
            }

            if (entity.SocialUser.Source == SocialUserSource.Twitter)
            {
                return Repository.FindAll().Any(t => t.IsDeleted == false && t.SocialUser.OriginalId == entity.SocialUser.OriginalId && entity.SocialUser.Source == SocialUserSource.Twitter);
            }

            return false;
        }
    }
}
