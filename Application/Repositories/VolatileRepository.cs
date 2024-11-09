namespace Application.Repositories;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class VolatileRepository<T, TKey> : IRepository<T?, TKey> where T : IEntity<TKey>
{
    private readonly ConcurrentDictionary<TKey, T> _entities = new();

    public Task<T?> GetByIdAsync(TKey id)
    {
        _entities.TryGetValue(id, out T? entity);
        return Task.FromResult(entity);
    }

    public Task AddAsync(T entity)
    {
        if (!_entities.TryAdd(entity.Id, entity))
            throw new ArgumentException($"An entity with Id {entity.Id} already exists.");

        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        if (!_entities.ContainsKey(entity.Id))
            throw new KeyNotFoundException($"Entity with Id {entity.Id} does not exist.");

        _entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(TKey id)
    {
        return Task.FromResult(_entities.TryRemove(id, out _));
    }
}
