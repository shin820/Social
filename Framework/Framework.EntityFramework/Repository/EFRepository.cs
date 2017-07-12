using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Framework.Core;

namespace Framework.EntityFramework
{
    public class EFRepository<TEntity> : ServiceBase, IRepository<TEntity> where TEntity : Entity
    {
        protected DbContext _dbContext;
        protected IDbSet<TEntity> DataSet;
        public EFRepository(DbContext dbContext)
        {
            DataSet = dbContext.Set<TEntity>();
            _dbContext = dbContext;
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
            _dbContext.SaveChanges();
        }

        public async Task InsertAsync(TEntity entity)
        {
            DataSet.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public void InsertMany(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                DataSet.Add(entity);
            }
            _dbContext.SaveChanges();
        }

        public async Task InsertManyAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                DataSet.Add(entity);
            }
            await _dbContext.SaveChangesAsync();
        }

        public void Delete(TEntity entity)
        {
            SoftDelete(entity);
            _dbContext.SaveChanges();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            SoftDelete(entity);
            await _dbContext.SaveChangesAsync();
        }

        public void DeleteMany(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                SoftDelete(entity);
            }
            _dbContext.SaveChanges();
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
            await _dbContext.SaveChangesAsync();
        }

        public void Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public void UpdateMany(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            _dbContext.SaveChanges();
        }

        public async Task UpdateManyAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            await _dbContext.SaveChangesAsync();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return FindAll().Where(predicate).Count();
        }
    }
}
