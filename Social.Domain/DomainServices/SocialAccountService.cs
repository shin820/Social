using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface ISocialAccountService : IDomainService<SocialAccount>
    {
        Task<SocialAccount> GetAccountAsync(SocialUserType type, string originalId);
    }

    public class SocialAccountService : DomainService<SocialAccount>, ISocialAccountService
    {
        public async Task<SocialAccount> GetAccountAsync(SocialUserType type, string originalId)
        {
            return await Repository.FindAll().Include(t => t.SocialUser).Where(t => t.SocialUser.OriginalId == originalId && t.SocialUser.Type == type && t.IfEnable).FirstOrDefaultAsync();
        }
    }
}
