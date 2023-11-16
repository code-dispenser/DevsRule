using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Events;

public class EventAggregatorTests
{
    public int FixedHandlerCallCount { get; set; } = 0;

    internal readonly EventAggregator _eventAggregator;

    public EventAggregatorTests()
    {
        _eventAggregator = new EventAggregator();
    }
    [Fact]
    public void Should_get_an_event_subscription_when_subscribing_to_an_event()
    {
        EventSubscription theEventSubscription = _eventAggregator.Subscribe<ConditionResultEvent>((_,_) => Task.CompletedTask);

        theEventSubscription.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_recieve_events_that_are_subscribed_to_using_fire_and_forget()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new());

        EventSubscription theEventSubscription = _eventAggregator.Subscribe<ConditionResultEvent>(HandleEvent);

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None, PublishMethod.FireAndForget);
        /*
            * Publish is using fire and forget, nothing to wait for so asserting in the handler
            * instead of using a Task.Deley(1) or something similar
        */
        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theEvent.SenderName.Should().Be("SomeSender");
            await Task.CompletedTask;

        }
    }
    [Fact]
    public async Task Should_recieve_events_that_are_subscribed_to_using_wait_for_all()
    {
        var theSenderName = String.Empty;

        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new());

        EventSubscription theEventSubscription = _eventAggregator.Subscribe<ConditionResultEvent>(HandleEvent);

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None, PublishMethod.WaitForAll);

        theSenderName.Should().Be("SomeSender");

        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theSenderName = theEvent.SenderName;
            await Task.CompletedTask;
        }
    }
    [Fact]
    public async Task Should_be_able_to_unsubscribe_from_events_by_disposing_the_subscription()
    {
        int theHandleCount  = 0;
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new());

        using (EventSubscription theEventSubscription = _eventAggregator.Subscribe<ConditionResultEvent>(HandleEvent))
        {
            await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None);
        }

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None);

        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theHandleCount++;
            await Task.CompletedTask;
        }

        await Task.Delay(1);

        theHandleCount.Should().Be(1);

    }

    [Fact]
    public async Task Nullified_objects_should_not_be_kept_alive_by_un_disposed_subscriptions()
    {
        ForRemoveUnregestered? testClass = new ForRemoveUnregestered(_eventAggregator, this);
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new());

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None);

        await Task.Delay(30);

        testClass = null;

        GC.Collect();

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None);
        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None);

        await Task.Delay(30);

        this.FixedHandlerCallCount.Should().Be(1);

    }

    public class ForRemoveUnregestered
    {
        private readonly EventSubscription _eventSubscription;
        private readonly EventAggregatorTests _parentClass;
        internal ForRemoveUnregestered(IEventAggregator eventAggregator, EventAggregatorTests parentClass)
        {
            _eventSubscription = eventAggregator.Subscribe<ConditionResultEvent>(FixedHandler);
            _parentClass = parentClass;
        }
        private async Task FixedHandler(ConditionResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            _parentClass.FixedHandlerCallCount++;
            await Task.CompletedTask;
        }
    }


}
