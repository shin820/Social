using Framework.EntityFramework.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.EntityFramework
{
    public class DbContextProvider<TDbContext> : IDbContextProvider<TDbContext>
        where TDbContext : DbContext
    {
        private ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;

        public DbContextProvider(ICurrentUnitOfWorkProvider currentUnitOfWorkProvider)
        {
            _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
        }

        public TDbContext GetDbContext()
        {
            return _currentUnitOfWorkProvider.Current.GetOrCreateDbContext<TDbContext>();
        }
    }

    public interface IDbContextProvider<TDbContext>
        where TDbContext : DbContext
    {
        TDbContext GetDbContext();
    }
}
