namespace Application.Interfaces.Common.IGenericRepository
{
    public interface ICommandRepository<T>
    {
        Task CreateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
