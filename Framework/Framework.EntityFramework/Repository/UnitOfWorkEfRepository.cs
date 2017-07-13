using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Framework.Core;

namespace Framework.EntityFramework
{
    public class UnitOfWorkEfRepository<TDbContext, TEntity> : ServiceBase, IRepository<TEntity>
        where TEntity : Entity
        where TDbContext : DbContext
    {
        protected IDbSet<TEntity> DataSet
        {
            get
            {
                return _dbContext.Set<TEntity>();
            }
        }

        private TDbContext _dbContext
        {
            get
            {
                return CurrentUnitOfWork.GetOrCreateDbContext<TDbContext>();
            }
        }

        public UnitOfWorkEfRepository()
        {
        }

        public IQueryable<TEntity> FindAll()
        {
            return DataSet;
        }

        public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate)
        {
            return FindAll().Where(predicate);
        }

        public TEntity Find(int id)
        {
            return DataSet.Find(id);
        }

        public void Insert(TEntity entity)
        {
            DataSet.Add(entity);
        }

        public async Task InsertAsync(TEntity entity)
        {
            DataSet.Add(entity);
        }

        public void InsertMany(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                DataSet.Add(entity);
            }
        }

        public async Task InsertManyAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                DataSet.Add(entity);
            }
        }

        public void Delete(TEntity entity)
        {
            SoftDelete(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            SoftDelete(entity);
        }

        public void DeleteMany(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                SoftDelete(entity);
            }
        }

        private void SoftDelete(TEntity entity)
        {
            var softDeleteEntity = entity as ISoftDelete;
            if (softDeleteEntity != null)
            {
                softDeleteEntity.IsDeleted = true;
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                DataSet.Remove(entity);
            }
        }

        public async Task DeleteManyAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                DataSet.Remove(entity);
            }
        }

        public void Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateMany(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        public async Task UpdateManyAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return FindAll().Where(predicate).Count();
        }
    }
}
