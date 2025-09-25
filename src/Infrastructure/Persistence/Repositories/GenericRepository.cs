using FCMS.Application.Abstracts.Repositories;
using FCMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FCMS.Persistence.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly FitnessDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(FitnessDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // ✅ Yeni əlavə: IQueryable<T> qaytarır, Include istifadə etmək üçün
        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with Id {id} not found.");
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
