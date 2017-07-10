using System.Data.Entity;

namespace Framework.EntityFramework.UnitOfWork
{
    public interface IUnitOfWork
    {
        string Id { get; }
        IUnitOfWork Outer { get; set; }
        bool IsDisposed { get; }
        void Begin();
        void Complete();
        void Dispose();
        TDbContext GetOrCreateDbContext<TDbContext>() where TDbContext : DbContext;
    }
}