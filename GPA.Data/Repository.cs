using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GPA.Data
{

    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly GPADbContext _context;
        private readonly DbSet<TEntity> _entitySet;
        public Repository(GPADbContext context)
        {
            _context = context;
            _entitySet = _context.Set<TEntity>();
        }

        public async Task<int> CountAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func, Expression<Func<TEntity, bool>>? expression = null)
        {
            if (expression == null)
            {
                return await func(_entitySet).CountAsync();
            }

            return await func(_entitySet).Where(expression).CountAsync();
        }

        public async Task<TEntity?> GetByIdAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func, Expression<Func<TEntity, bool>>? expression)
        {
            if (expression is not null)
            {
                return await func(_entitySet).FirstOrDefaultAsync(expression);
            }
            return await func(_entitySet).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func, Expression<Func<TEntity, bool>>? expression)
        {
            if (expression == null)
            {
                return await func(_entitySet).ToListAsync();
            }

            return await func(_entitySet).Where(expression).ToListAsync();
        }

        public async Task<TEntity?> AddAsync(TEntity entity)
        {
            if (entity is not null)
            {
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            return null;
        }

        public async Task UpdateAsync(TEntity model, TEntity entity, Action<EntityEntry<TEntity>, TEntity>? action = null)
        {
            if (model is not null)
            {
                var entityEntry = _entitySet.Update(entity);
                action?.Invoke(entityEntry, model);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(TEntity model)
        {
            if (model is not null)
            {
                _context.Remove(model);
                await _context.SaveChangesAsync();
            }
        }
    }
}
