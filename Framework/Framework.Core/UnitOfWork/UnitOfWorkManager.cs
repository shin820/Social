using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Framework.Core.UnitOfWork
{
    public class UnitOfWorkManager : IUnitOfWorkManager, ITransient
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;

        public IUnitOfWork Current
        {
            get { return _currentUnitOfWorkProvider.Current; }
        }

        public UnitOfWorkManager(
            IDependencyResolver dependencyResolver,
            ICurrentUnitOfWorkProvider currentUnitOfWorkProvider
            )
        {
            _dependencyResolver = dependencyResolver;
            _currentUnitOfWorkProvider = currentUnitOfWorkProvider;
        }

        public IUnitOfWorkCompleteHandle Begin()
        {
            return Begin(new UnitOfWorkOptions());
        }

        public IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope)
        {
            return Begin(new UnitOfWorkOptions { Scope = scope });
        }

        public IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options)
        {
            options = options ?? new UnitOfWorkOptions();

            var outerUow = _currentUnitOfWorkProvider.Current;
            if (options.Scope == TransactionScopeOption.Required && outerUow != null)
            {
                return new InnerUnitOfWorkCompleteHandle();
            }

            var uow = _dependencyResolver.Resolve<IUnitOfWork>();

            uow.Completed += (sender, args) =>
            {
                _currentUnitOfWorkProvider.Current = null;
            };

            uow.Failed += (sender, args) =>
            {
                _currentUnitOfWorkProvider.Current = null;
            };

            uow.Disposed += (sender, args) =>
            {
                _dependencyResolver.Release(uow);
            };

            uow.Begin(options);

            _currentUnitOfWorkProvider.Current = uow;

            return uow;
        }
    }
}
