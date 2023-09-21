using Application.Interfaces.Common.IGenericRepository;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.CommonRepo.GenericRepository
{
    public class CommandRepository<T> : ICommandRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public CommandRepository(AppDbContext context) 
        { 
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task CreateAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}