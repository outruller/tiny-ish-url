namespace Application.Repositories;

public interface IEntity<TKey> where TKey: notnull
{
    TKey Id { get; set; }
}