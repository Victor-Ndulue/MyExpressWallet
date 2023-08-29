using System.Linq.Expressions;

namespace Application.Interfaces.Common.IGenericRepository
{
    public interface IQueryRepository<T>
    {
        IQueryable<T> GetByCondition(Expression<Func<T, bool>> conditionExpression, bool trackChanges);
        IQueryable<T> GetAll(bool trackChanges);
        Task<bool> CheckAnyAsync(Expression<Func<T, bool>> predicate);
    }
}
