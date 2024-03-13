using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using FluentAssertions.Execution;
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
    public async Task Should_receive_events_that_are_subscribed_to_using_fire_and_forget()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new());

        EventSubscription theEventSubscription = _eventAggregator.Subscribe<ConditionResultEvent>(HandleEvent);

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None, PublishMethod.FireAndForget);
        /*
            * Publish is using fire and forget, nothing to wait for so asserting in the handler
            * instead of using a Task.Delay(1) or something similar
        */
        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theEvent.SenderName.Should().Be("SomeSender");
            await Task.CompletedTask;

        }
    }
    [Fact]
    public async Task Should_receive_events_that_are_subscribed_to_using_wait_for_all()
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
            await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None, PublishMethod.WaitForAll);
        }

        await _eventAggregator.Publish(conditionResultEvent, CancellationToken.None);

        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theHandleCount++;
            await Task.CompletedTask;
        }

        theHandleCount.Should().Be(1);

    }

    [Fact]
    public async Task Nullified_objects_should_not_be_kept_alive_by_un_disposed_subscriptions()
    {
        ForRemoveUnregistered? testClass = new ForRemoveUnregistered(_eventAggregator, this);
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



    [Fact]
    public void The_weak_reference_comparer_should_return_true_for_identical_references()
    {
        var theComparer       = new EventAggregator.WeakReferenceDelegateComparer();
        var weakRefHandlerOne = new WeakReference<Delegate>(RuleEventHandler);
        var weakRefHandlerTwo = new WeakReference<Delegate>(RuleEventHandler);

        theComparer.Equals(weakRefHandlerOne, weakRefHandlerTwo).Should().BeTrue();

        async Task RuleEventHandler(RuleResultEvent ruleEvent, CancellationToken cancellationToken) { await Task.CompletedTask; }
    }
    [Fact]
    public void The_weak_reference_comparer_should_return_false_for_different_references()
    {
        var theComparer = new EventAggregator.WeakReferenceDelegateComparer();
        var weakRefHandlerOne = new WeakReference<Delegate>(RuleEventHandlerOne);
        var weakRefHandlerTwo = new WeakReference<Delegate>(RuleEventHandlerTwo);

        theComparer.Equals(weakRefHandlerOne, weakRefHandlerTwo).Should().BeFalse();

        async Task RuleEventHandlerOne(RuleResultEvent ruleEvent, CancellationToken cancellationToken) { await Task.CompletedTask; }
        async Task RuleEventHandlerTwo(RuleResultEvent ruleEvent, CancellationToken cancellationToken) { await Task.CompletedTask; }
    }

    [Fact]
    public void The_weak_reference_comparer_should_return_false_for_a_null_reference()
    {
        var theComparer = new EventAggregator.WeakReferenceDelegateComparer();
        WeakReference<Delegate>? weakRefHandlerOne = null;
        var weakRefHandlerTwo = new WeakReference<Delegate>(RuleEventHandlerTwo);

        theComparer.Equals(weakRefHandlerOne, weakRefHandlerTwo).Should().BeFalse();

        async Task RuleEventHandlerTwo(RuleResultEvent ruleEvent, CancellationToken cancellationToken) { await Task.CompletedTask; }
    }

    [Fact]
    public void The_weak_reference_comparer_should_return_false_for_a_null_reference_target()
    {
        var theComparer = new EventAggregator.WeakReferenceDelegateComparer();
        WeakReference<Delegate>? weakRefHandlerOne = new WeakReference<Delegate>(null!);
        var weakRefHandlerTwo = new WeakReference<Delegate>(RuleEventHandlerTwo);

        theComparer.Equals(weakRefHandlerOne, weakRefHandlerTwo).Should().BeFalse();

        async Task RuleEventHandlerTwo(RuleResultEvent ruleEvent, CancellationToken cancellationToken) { await Task.CompletedTask; }
    }

    [Fact]
    public void The_weak_reference_comparer_get_hash_code_should_return_an_integer_value()
    {
        var theComparer = new EventAggregator.WeakReferenceDelegateComparer();
        var weakRefHandlerOne = new WeakReference<Delegate>(RuleEventHandler);
        var weakRefHandlerTwo = new WeakReference<Delegate>(RuleEventHandler);

        var theCodeValue = theComparer.GetHashCode(weakRefHandlerOne);

        using (new AssertionScope())
        {
            theCodeValue.Should().BeGreaterThan(0);
            theCodeValue.Should().Be(theComparer.GetHashCode(weakRefHandlerTwo));
        }

        async Task RuleEventHandler(RuleResultEvent ruleEvent, CancellationToken cancellationToken) { await Task.CompletedTask; }
    }


    private class ForRemoveUnregistered
    {
        private readonly EventSubscription _eventSubscription;
        private readonly EventAggregatorTests _parentClass;
        internal ForRemoveUnregistered(IEventAggregator eventAggregator, EventAggregatorTests parentClass)
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
