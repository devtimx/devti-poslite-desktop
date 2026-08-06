using System.Linq.Expressions;

namespace DevtiPosLite.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(uint id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdWithIncludeAsync(uint id, params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
}
