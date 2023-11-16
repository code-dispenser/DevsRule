namespace DevsRule.Core.Areas.Events;

/// <summary>
/// An object to handle events raised by either a rule or conditions within a rule.
/// </summary>
/// <typeparam name="TEvent">The type of event that is handled.</typeparam>
public interface IEventHandler<TEvent> where TEvent : IEvent
{

    /// <summary>
    /// The method to handle the event.
    /// </summary>
    /// <param name="theEvent">The type of event to handle.</param>
    /// <param name="cancellationToken">A cancellation token used to signify a cancellation request.</param>
    /// <returns></returns>
    public Task Handle(TEvent theEvent, CancellationToken cancellationToken);
}
