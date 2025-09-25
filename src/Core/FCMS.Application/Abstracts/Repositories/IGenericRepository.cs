using System.Linq.Expressions;

namespace FCMS.Application.Abstracts.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetQueryable();   // <-- Əlavə et

    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Remove(T entity);
    void Update(T entity);
    Task<int> SaveChangesAsync();
}
