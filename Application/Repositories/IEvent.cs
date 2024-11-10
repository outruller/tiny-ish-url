namespace Application.Repositories;

public interface IEvent
{
    Guid Id { get; }
    DateTime Timestamp { get; }
}
