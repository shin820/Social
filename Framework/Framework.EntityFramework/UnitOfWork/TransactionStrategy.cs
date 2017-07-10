using Framework.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Framework.EntityFramework.UnitOfWork
{
    public class TransactionStrategy : ITransactionStrategy
    {
        protected TransactionScope CurrentTransaction { get; set; }

        protected List<DbContext> DbContexts { get; }

        private IDependencyResolver _dependencyResolver;

        public TransactionStrategy(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
            DbContexts = new List<DbContext>();
        }

        public DbContext CreateDbContext<TDbContext>(string connectionString)
         where TDbContext : DbContext
        {
            var dbContext = _dependencyResolver.Resolve<TDbContext>(connectionString);
            DbContexts.Add(dbContext);
            return dbContext;
        }

        public void StartTransaction()
        {
            if (CurrentTransaction != null)
            {
                return;
            }

            CurrentTransaction = new TransactionScope(
                TransactionScopeAsyncFlowOption.Enabled
            );
        }

        public void Commit()
        {
            if (CurrentTransaction == null)
            {
                return;
            }

            CurrentTransaction.Complete();

            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public void Dispose()
        {
            foreach (var dbContext in DbContexts)
            {
                _dependencyResolver.Release(dbContext);
            }

            DbContexts.Clear();

            if (CurrentTransaction != null)
            {
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }
        }
    }
}
