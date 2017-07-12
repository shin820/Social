using Framework.Core;
using Framework.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain
{
    public class SiteDataRepository<TEntity> : UnitOfWorkEfRepository<SiteDataContext, TEntity>, IRepository<TEntity> where TEntity : Entity
    {
    }
}
