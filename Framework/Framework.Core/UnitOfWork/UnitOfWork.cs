using Castle.Core;
using Framework.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Framework.Core.UnitOfWork
{
    public class UnitOfWork : IDisposable, IUnitOfWork, ITransient
    {
        public string Id { get; }

        [DoNotWire]
        public IUnitOfWork Outer { get; set; }

        public UnitOfWorkOptions Options { get; private set; }

        private IDependencyResolver _dependencyResolver;
        private IDictionary<string, DbContext> _activeDbContexts;
        private ITransactionStrategy _transactionStrategy;

        public bool IsDisposed { get; private set; }
        private bool _isBeginCalled;
        private bool _isCompleteCalled;
        private bool _isSucceed;

        public event EventHandler Completed;
        public event EventHandler<UnitOfWorkFailedEventArgs> Failed;
        public event EventHandler Disposed;

        private Exception _exception;

        private readonly IConnectionStringResolver _connectionStringResolver;
        private readonly IDbContextResolver _dbContextResolver;
        private readonly IUserContext _userContext;

        private int? _siteId;

        public UnitOfWork(
            IConnectionStringResolver connectionStringResolver,
            IDbContextResolver dbContextResolver,
            IDependencyResolver dependencyResolver,
            ITransactionStrategy transactionStrategy,
            IUserContext userContext)
        {
            Id = Guid.NewGuid().ToString("N");
            _dependencyResolver = dependencyResolver;
            _transactionStrategy = transactionStrategy;
            _connectionStringResolver = connectionStringResolver;
            _dbContextResolver = dbContextResolver;
            _activeDbContexts = new Dictionary<string, DbContext>();
            _userContext = userContext;
        }

        public virtual TDbContext GetOrCreateDbContext<TDbContext>() where TDbContext : DbContext
        {
            var connectionString = _connectionStringResolver.GetNameOrConnectionStringForSiteDb(_siteId.GetValueOrDefault());
            var dbContextKey = typeof(TDbContext).FullName + "#" + connectionString;

            DbContext dbContext;
            if (!_activeDbContexts.TryGetValue(dbContextKey, out dbContext))
            {
                if (Options.IsTransactional)
                {
                    dbContext = _transactionStrategy.CreateDbContext<TDbContext>(connectionString);
                }
                else
                {
                    dbContext = _dbContextResolver.Resolve<TDbContext>(connectionString);
                }

                if (Options.Timeout.HasValue && !dbContext.Database.CommandTimeout.HasValue)
                {
                    dbContext.Database.CommandTimeout = (int)Convert.ChangeType(Options.Timeout.Value.TotalSeconds, typeof(int));
                }

                _activeDbContexts[dbContextKey] = dbContext;
            }
            return (TDbContext)dbContext;
        }

        public void Begin(UnitOfWorkOptions options)
        {
            PreventMultipleBegin();

            Options = options;

            SetSiteId(_userContext.SiteId);

            if (Options.IsTransactional)
            {
                _transactionStrategy.StartTransaction(Options);
            }
        }

        private void PreventMultipleBegin()
        {
            if (_isBeginCalled)
            {
                throw new NotSupportedException("This unit of work has started before.");
            }
            _isBeginCalled = true;
        }

        public void Complete()
        {
            if (_isCompleteCalled)
            {
                throw new NotSupportedException("This unit of work has completed.");
            }

            try
            {
                SaveChanges();
                _isCompleteCalled = true;

                if (Options.IsTransactional)
                {
                    _transactionStrategy.Commit();
                }

                _isSucceed = true;
                OnCompleted();
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw;
            }
        }

        public void Dispose()
        {
            if (!_isBeginCalled || IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            if (!_isSucceed)
            {
                OnFailed(_exception);
            }

            if (Options.IsTransactional)
            {
                _transactionStrategy.Dispose();
            }
            else
            {
                foreach (var dbContext in GetAllActiveDbContexts())
                {
                    dbContext.Dispose();
                    _dependencyResolver.Release(dbContext);
                }
            }

            _activeDbContexts.Clear();

            OnDisposed();
        }

        public void SaveChanges()
        {
            // _activeDbContexts.Values.ToImmutableList()
            foreach (var dbContext in GetAllActiveDbContexts())
            {
                dbContext.SaveChanges();
            }
        }

        public async Task SaveChangesAsync()
        {
            foreach (var dbContext in GetAllActiveDbContexts())
            {
                await dbContext.SaveChangesAsync();
            }
        }

        public override string ToString()
        {
            return $"[UnitOfWork {Id}]";
        }

        protected virtual void OnCompleted()
        {
            if (Completed == null)
            {
                return;
            }

            Completed(this, EventArgs.Empty);
        }

        protected virtual void OnDisposed()
        {
            if (Disposed == null)
            {
                return;
            }

            Disposed(this, EventArgs.Empty);
        }

        protected virtual void OnFailed(Exception exception)
        {
            if (Failed == null)
            {
                return;
            }

            Failed(this, new UnitOfWorkFailedEventArgs(exception));
        }

        public IReadOnlyList<DbContext> GetAllActiveDbContexts()
        {
            return _activeDbContexts.Values.ToImmutableList();
        }

        public IDisposable SetSiteId(int? siteId)
        {
            var oldSiteId = _siteId;
            _siteId = siteId;

            return new DisposeAction(() =>
            {
                _siteId = oldSiteId;
            });
        }

        public int? GetSiteId()
        {
            return _siteId;
        }
    }
}
