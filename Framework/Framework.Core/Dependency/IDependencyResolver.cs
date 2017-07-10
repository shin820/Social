using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core
{
    public interface IDependencyResolver
    {
        T Resolve<T>();

        T Resolve<T>(Type type);

        T Resolve<T>(object argumentsAsAnonymousType);

        object Resolve(Type type);

        object Resolve(Type type, object argumentsAsAnonymousType);

        T[] ResolveAll<T>();

        T[] ResolveAll<T>(object argumentsAsAnonymousType);

        object[] ResolveAll(Type type);

        object[] ResolveAll(Type type, object argumentsAsAnonymousType);

        void Release(object obj);

        IDisposable BeginScope();

        bool IsRegistered(Type type);

        bool IsRegistered<T>();
    }
}
