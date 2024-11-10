namespace Application.Repositories;

using System.Collections.Concurrent;

using Application.Model;

public class VolatileAnalyticsEventStore : IAnalyticsEventStore
{
  private readonly ConcurrentBag<(string ShortUrl, IAnalyticsEvent Event)> _events = [];

  public void AppendEvent(string shortUrl, IAnalyticsEvent @event)
  {
    _events.Add((shortUrl, @event));
  }

  public IEnumerable<IAnalyticsEvent> GetEvents(string shortUrl)
  {
    return _events
        .Where(e => e.ShortUrl == shortUrl)
        .Select(e => e.Event)
        .OrderBy(e => e.Timestamp);
  }
}