using Application.Interfaces.Common.IGenericRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Repositories.CommonRepo.GenericRepository
{
    public class QueryRepository<T> : IQueryRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;

        public QueryRepository(AppDbContext context) 
        {
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> GetByCondition (Expression<Func<T, bool>> conditionExpression, bool trackChanges)
        {
            var query = _dbSet.AsQueryable();
            if (!trackChanges) query = query.AsNoTracking();
            return query.Where(conditionExpression);
        }

        public IQueryable<T> GetAll(bool trackChanges) 
        {
            var query = _dbSet.AsQueryable();
            return !trackChanges? query.AsNoTracking() : query;
        }

        public Task<bool> CheckAnyAsync(Expression <Func<T, bool> > predicate)
        {
            var query = _dbSet.AnyAsync(predicate);
            return query;
        }

    }
}