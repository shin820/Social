using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.EntityFramework.UnitOfWork
{
    public class UnitOfWorkManager : IUnitOfWorkManager
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;

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
            var outerUow = _currentUnitOfWorkProvider.Current;
            if (outerUow != null)
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

            uow.Begin();

            _currentUnitOfWorkProvider.Current = uow;

            return uow;
        }
    }
}
