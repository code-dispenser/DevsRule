using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Events;

internal interface IEventAggregator
{
    Task Publish<TEvent>(TEvent theEvent, CancellationToken cancellationToken, PublishMethod publishMethod = PublishMethod.FireAndForget) where TEvent : IEvent;
    EventSubscription Subscribe<TEvent>(HandleEvent<TEvent> handler) where TEvent : IEvent;
}
