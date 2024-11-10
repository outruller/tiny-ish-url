namespace Application.Repositories;

public interface IEventStore<TKey, TEvent>
{
    void AppendEvent(TKey key, TEvent @event);
    IEnumerable<TEvent> GetEvents(TKey key);
}
