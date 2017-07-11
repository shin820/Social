using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.EntityFramework.UnitOfWork
{
    public interface IUnitOfWorkCompleteHandle : IDisposable
    {
        void Complete();
    }
}
