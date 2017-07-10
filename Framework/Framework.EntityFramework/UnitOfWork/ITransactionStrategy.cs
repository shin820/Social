using System.Data.Entity;

namespace Framework.EntityFramework.UnitOfWork
{
    public interface ITransactionStrategy
    {
        void Commit();
        DbContext CreateDbContext<TDbContext>(string connectionString) where TDbContext : DbContext;
        void Dispose();
        void StartTransaction();
    }
}