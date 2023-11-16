namespace DevsRule.Core.Areas.Events;

/// <summary>
/// An EventSubscription keeps a reference to an event that you have registered to receive.
/// To stop receiving published events you can call the Dispose method. The subscription uses weakreferences and as such
/// will be automatically removed once the subscription goes out of scope if dispose is not called.
/// </summary>
public class EventSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private readonly Delegate _handler;

    private bool _disposed = false;

    /*
        * Need to store the actual delegate handler as otherwise due to the weakreferences there are no references to it and it just gets
        * gc collected and automatically removed from the subscriptions 
    */
    public EventSubscription(Action removeAction, Delegate handler) => (_unsubscribe, _handler) = (removeAction, handler);

    public void Dispose()
    {
        if (false == _disposed)
        {
            _disposed = true;
            _unsubscribe?.Invoke();
        }
    }
}
