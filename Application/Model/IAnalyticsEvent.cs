
namespace Application.Model;

using Application.Repositories;

public interface IAnalyticsEvent : IEvent
{
  AnalyticsEventType EventType { get; }
}
