using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GPA.Data
{

    public interface IRepository<TEntity> where TEntity : class
    {
        Task<int> CountAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func, Expression<Func<TEntity, bool>>? expression = null);
        Task<TEntity?> GetByIdAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func, Expression<Func<TEntity, bool>>? expression = null);
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func, Expression<Func<TEntity, bool>>? expression = null);
        Task<TEntity?> AddAsync(TEntity entity);
        Task UpdateAsync(TEntity model, TEntity entity, Action<EntityEntry<TEntity>, TEntity>? action = null);
        Task RemoveAsync(TEntity model);
    }
}
