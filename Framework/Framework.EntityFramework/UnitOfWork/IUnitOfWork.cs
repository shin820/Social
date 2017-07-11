using System;
using System.Data.Entity;

namespace Framework.EntityFramework.UnitOfWork
{
    public interface IUnitOfWork : IUnitOfWorkCompleteHandle
    {
        string Id { get; }
        IUnitOfWork Outer { get; set; }
        bool IsDisposed { get; }
        void Begin();
        TDbContext GetOrCreateDbContext<TDbContext>() where TDbContext : DbContext;

        event EventHandler Completed;

        event EventHandler<UnitOfWorkFailedEventArgs> Failed;

        event EventHandler Disposed;
    }
}