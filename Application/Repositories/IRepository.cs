namespace Application.Repositories;

public interface IRepository<T, TKey> where T : IEntity<TKey>
{
    Task<T?> GetByIdAsync(TKey id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
}