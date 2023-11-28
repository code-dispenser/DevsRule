using DevsRule.Core.Common.Seeds;
using System.Collections.Concurrent;

namespace DevsRule.Core.Areas.Events;

/// <inheritdoc cref="IEventAggregator" />
internal class EventAggregator : IEventAggregator
{
    private readonly ConcurrentDictionary<Type, List<WeakReference<Delegate>>> _eventSubscriptions = new();

    private readonly Func<Type, dynamic>? _resolver = null;

    private readonly bool _dependencyInjectionEnabled = false;

    public EventAggregator(Func<Type, object> resolver)

        => (_resolver, _dependencyInjectionEnabled) = (resolver, true);

    public EventAggregator()

        => (_resolver, _dependencyInjectionEnabled) = (null, false);

    private void Unsubscribe(Type eventType, WeakReference<Delegate> handler)
    {
        if (true == _eventSubscriptions.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);

            if (handlers.Count == 0) _eventSubscriptions.TryRemove(eventType, out _);
        }
    }
    public async Task Publish<TEvent>(TEvent conditionRuleEvent, CancellationToken cancellationToken, PublishMethod publishMethod = PublishMethod.FireAndForget) where TEvent : IEvent
    {
        List<HandleEvent<TEvent>> eventHandlers = new();

        eventHandlers.AddRange(GetSubscribedHandlers<TEvent>());

        if (true == _dependencyInjectionEnabled) eventHandlers.AddRange(GetRegisteredHandlers<TEvent>(cancellationToken));

        if (publishMethod == PublishMethod.FireAndForget) FireAndForgetStategy(conditionRuleEvent,eventHandlers, cancellationToken);

        if (publishMethod == PublishMethod.WaitForAll) await WaitAllStategy(conditionRuleEvent, eventHandlers, cancellationToken).ConfigureAwait(false);


    }

    private async Task WaitAllStategy<TEvent>(TEvent conditionRuleEvent, List<HandleEvent<TEvent>> eventHandlers, CancellationToken cancellationToken) where TEvent : IEvent
    {
        List<Task> tasks = new();

        try
        {
            foreach (var handler in eventHandlers)
            {
                tasks.Add(handler(conditionRuleEvent, cancellationToken));
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch { }
    }

    private void FireAndForgetStategy<TEvent>(TEvent conditionRuleEvent, List<HandleEvent<TEvent>> eventHandlers, CancellationToken cancellationToken) where TEvent : IEvent
    {
        foreach (var handler in eventHandlers)
        {
           _ = Task.Run(async () => await handler(conditionRuleEvent, cancellationToken), cancellationToken);//Fire and forget
        }
    }

    public EventSubscription Subscribe<TEvent>(HandleEvent<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);

        var handlerList     = _eventSubscriptions.GetOrAdd(eventType, _ => new());
        var weakRefHandler  = new WeakReference<Delegate>(handler);
        var comparer        = new WeakReferenceDelegateComparer();

        if (false == handlerList.Any(existing => comparer.Equals(existing, weakRefHandler))) handlerList.Add(weakRefHandler);

        return new EventSubscription(() => Unsubscribe(eventType, weakRefHandler), handler);
    }



    private List<HandleEvent<TEvent>> GetRegisteredHandlers<TEvent>(CancellationToken cancellationToken) where TEvent : IEvent
    {
        List<HandleEvent<TEvent>> registeredHandlers = new();

        var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(typeof(TEvent));

        try
        {

            var enumerableHandlersType  = typeof(IEnumerable<>).MakeGenericType(eventHandlerType);
            var domaninEventHandlers    = _resolver!(enumerableHandlersType) as Array;

            foreach (var eventHandler in domaninEventHandlers!)
            {
                try
                {
                    HandleEvent<TEvent> handler = (TEvent,_) => ((dynamic)eventHandler).Handle(TEvent, cancellationToken);

                    registeredHandlers.Add(handler);
                }
                catch { }//TODO, convertion errors should be caught on import so probaly ok? 
            }
            
        }
        catch //TODO what to do about Not Registered errors other than squash them?
        {
            return registeredHandlers;
        }
        return registeredHandlers;
    }

    private List<HandleEvent<TEvent>> GetSubscribedHandlers<TEvent>() where TEvent: IEvent
    {
        List<HandleEvent<TEvent>> eventHandlers = new();

        List<WeakReference<Delegate>> subscribedHandlers;

        subscribedHandlers = _eventSubscriptions.TryGetValue(typeof(TEvent), out subscribedHandlers!) == true ? subscribedHandlers.ToList() : new List<WeakReference<Delegate>>();

        foreach (var subscribedHandler in subscribedHandlers)
        {
            if ((subscribedHandler.TryGetTarget(out Delegate? target)) && (target is HandleEvent<TEvent> handler))
            {
                eventHandlers.Add(handler);
            }
            else { Task.Run(() => Unsubscribe(typeof(TEvent), subscribedHandler)); }
        }

        return eventHandlers;
    }

    internal class WeakReferenceDelegateComparer : IEqualityComparer<WeakReference<Delegate>>
    {
        public bool Equals(WeakReference<Delegate>? x, WeakReference<Delegate>? y)
        {
            if (x == null || y == null) return false;

            Delegate? xTarget, yTarget;
            if (false == x.TryGetTarget(out xTarget) || false == y.TryGetTarget(out yTarget)) return false;

            return xTarget == yTarget || xTarget.Equals(yTarget);
        }

        public int GetHashCode(WeakReference<Delegate> obj)
        {
            Delegate? target;
            return obj.TryGetTarget(out target) && target != null ? target.GetHashCode() : obj.GetHashCode();
        }
    }
}


