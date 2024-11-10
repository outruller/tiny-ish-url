namespace Application.Services;

using Application.Model;
using Application.Repositories;

public class AnalyticsService(
  IAnalyticsEventStore _analyticsEventStore
)
{
  public int Count(string key) => _analyticsEventStore.GetEvents(key)
    .Where(e => e.EventType == AnalyticsEventType.CLICK)
    .Count();

  public void Hit(string key)
  {
    // Fire and forget for analytics
    Task.Factory.StartNew(() =>
    {
      _analyticsEventStore.AppendEvent(key, new ClickEvent());
    });
  }
}