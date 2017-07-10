using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Core
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        void Delete(TEntity entity);
        Task DeleteAsync(TEntity entity);
        void DeleteMany(TEntity[] entities);
        Task DeleteManyAsync(TEntity[] entities);
        TEntity Find(int id);
        IQueryable<TEntity> FindAll();
        IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate);
        void Insert(TEntity entity);
        Task InsertAsync(TEntity entity);
        void InsertMany(TEntity[] entities);
        Task InsertManyAsync(TEntity[] entities);
        void Update(TEntity entity);
        Task UpdateAsync(TEntity entity);
        void UpdateMany(TEntity[] entities);
        Task UpdateManyAsync(TEntity[] entities);
        int Count(Expression<Func<TEntity, bool>> predicate);
    }
}