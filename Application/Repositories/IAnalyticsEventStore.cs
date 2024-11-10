namespace Application.Repositories;

using Application.Model;

public interface IAnalyticsEventStore: IEventStore<string, IAnalyticsEvent>;