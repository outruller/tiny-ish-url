
namespace Application.Model;

class ClickEvent : IAnalyticsEvent
{
  public AnalyticsEventType EventType => AnalyticsEventType.CLICK;

  public Guid Id => Guid.NewGuid();

  public DateTime Timestamp => DateTime.UtcNow;
}
