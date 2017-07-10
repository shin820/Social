using Castle.Core;
using Framework.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;

namespace Framework.EntityFramework.UnitOfWork
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        public string Id { get; }

        [DoNotWire]
        public IUnitOfWork Outer { get; set; }

        private IDependencyResolver _dependencyResolver;
        private IDictionary<string, DbContext> _activeDbContexts;
        private ITransactionStrategy _transactionStrategy;

        public bool IsDisposed { get; private set; }
        private bool _isBeginCalled;
        private bool _isCompleteCalled;

        public UnitOfWork(
            IDependencyResolver dependencyResolver,
            ITransactionStrategy transactionStrategy)
        {
            Id = Guid.NewGuid().ToString("N");
            _dependencyResolver = dependencyResolver;
            _transactionStrategy = transactionStrategy;
            _activeDbContexts = new Dictionary<string, DbContext>();
        }

        public virtual TDbContext GetOrCreateDbContext<TDbContext>() where TDbContext : DbContext
        {
            var connectionString = ConfigurationManager.ConnectionStrings["KBDataContext"].ConnectionString;
            var dbContextKey = typeof(TDbContext).FullName + "#" + connectionString;

            DbContext dbContext;
            if (!_activeDbContexts.TryGetValue(dbContextKey, out dbContext))
            {
                dbContext = _transactionStrategy.CreateDbContext<TDbContext>(connectionString);
                _activeDbContexts[dbContextKey] = dbContext;
            }
            return (TDbContext)dbContext;
        }

        public void Begin()
        {
            if (_isBeginCalled)
            {
                throw new NotSupportedException("This unit of work has started before.");
            }

            _isBeginCalled = true;
            _transactionStrategy.StartTransaction();
        }

        public void Complete()
        {
            if (_isCompleteCalled)
            {
                throw new NotSupportedException("This unit of work has completed.");
            }

            SaveChanges();
            _isCompleteCalled = true;
            _transactionStrategy.Commit();
        }

        public void Dispose()
        {
            if (!_isBeginCalled || IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            _transactionStrategy.Dispose();
        }

        private void SaveChanges()
        {
            // _activeDbContexts.Values.ToImmutableList()
            foreach (var dbContext in _activeDbContexts.Values)
            {
                dbContext.SaveChanges();
            }
        }

        public override string ToString()
        {
            return $"[UnitOfWork {Id}]";
        }
    }
}
