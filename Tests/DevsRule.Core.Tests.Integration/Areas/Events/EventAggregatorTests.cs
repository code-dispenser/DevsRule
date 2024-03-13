using Autofac;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Sdk;

namespace DevsRule.Core.Tests.Integration.Areas.Events;

public class EventAggregatorTests
{ 
    internal IEventAggregator _eventAggregator;

    public int MyHandlerCallCount  { get; set; } = 0;

    

    public EventAggregatorTests()
    {
        _eventAggregator = ConfigureAutofac().Resolve<IEventAggregator>();
    }

    private IContainer ConfigureAutofac()
    {

        var builder = new ContainerBuilder();

        builder.RegisterType<MyHandler>().As<IEventHandler<ConditionResultEvent>>();
        builder.RegisterInstance(this).SingleInstance();
        builder.Register<IEventAggregator>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return new EventAggregator(type => context.Resolve(type));
        }).SingleInstance();


        return builder.Build();
    }

    [Fact]
    public async Task DI_registered_event_handlers_should_be_able_to_be_called_using_fire_and_forget()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), null);

        await _eventAggregator.Publish(conditionResultEvent,CancellationToken.None, PublishMethod.FireAndForget);
        /*
            * publish is fire and forget so need a delay for the handler to be created before checking, set higher than needed 
         */
        await Task.Delay(50);
        this.MyHandlerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task DI_registered_event_handlers_should_be_able_to_be_called_using_wait_for_all()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), null);

        await _eventAggregator.Publish(conditionResultEvent,CancellationToken.None, PublishMethod.WaitForAll);

        this.MyHandlerCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_squash_un_handled_wait_for_all_errors_in_handlers()
    {

        var ruleEvent = new RuleResultEvent("TheRule", false, String.Empty, String.Empty, GlobalStrings.Default_TenantID, new List<Exception>());
        var subscription = _eventAggregator.Subscribe<RuleResultEvent>(BadHandler);

        await FluentActions.Invoking(() => _eventAggregator.Publish(ruleEvent, CancellationToken.None, PublishMethod.WaitForAll)).Should().NotThrowAfterAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMicroseconds(50));

        async Task BadHandler(RuleResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(25));
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task Disposing_an_event_subscription_should_only_remove_the_handler_from_the_event_subscription_list_not_the_event_type()
    {
        var ruleEvent = new RuleResultEvent("TheRule", false, String.Empty, String.Empty, GlobalStrings.Default_TenantID, new List<Exception>());
        var handlerOneSub = _eventAggregator.Subscribe<RuleResultEvent>(handlerOne);
        var handlerTwoSub = _eventAggregator.Subscribe<RuleResultEvent>(handlerTwo);

        handlerTwoSub.Dispose();

        await _eventAggregator.Publish(ruleEvent, CancellationToken.None, PublishMethod.WaitForAll);

        this.MyHandlerCallCount.Should().Be(1);

        async Task handlerOne(RuleResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            this.MyHandlerCallCount++;

            await Task.CompletedTask;
        }

        async Task handlerTwo(RuleResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            this.MyHandlerCallCount++;

            await Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Trying_to_add_a_duplicate_handler_should_just_return_a_subscription_without_adding_a_second_handler()
    {
        var ruleEvent = new RuleResultEvent("TheRule", false, String.Empty, String.Empty, GlobalStrings.Default_TenantID, new List<Exception>());
        var handlerOneSub = _eventAggregator.Subscribe<RuleResultEvent>(handlerOne);
        var duplicateSub  = _eventAggregator.Subscribe<RuleResultEvent>(handlerOne);

        await _eventAggregator.Publish(ruleEvent, CancellationToken.None, PublishMethod.WaitForAll);
        var firstCount = this.MyHandlerCallCount;

        handlerOneSub.Dispose();
        
        await _eventAggregator.Publish(ruleEvent, CancellationToken.None, PublishMethod.WaitForAll);
        var secondCount = this.MyHandlerCallCount;

        duplicateSub.Dispose();

        using(new AssertionScope())
        {
            firstCount.Should().Be(1);
            secondCount.Should().Be(firstCount);
        }

        async Task handlerOne(RuleResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            this.MyHandlerCallCount++;

            await Task.CompletedTask;
        }

    }
    [Fact]
    public void A_duplicate_sub_should_not_error_when_disposing()
    {
        var ruleEvent = new RuleResultEvent("TheRule", false, String.Empty, String.Empty, GlobalStrings.Default_TenantID, new List<Exception>());
        var handlerOneSub = _eventAggregator.Subscribe<RuleResultEvent>(handlerOne);
        var duplicateSub = _eventAggregator.Subscribe<RuleResultEvent>(handlerOne);

        handlerOneSub.Dispose();

        FluentActions.Invoking(() => duplicateSub.Dispose()).Should().NotThrow();

        async Task handlerOne(RuleResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

    }
    public class MyHandler : IEventHandler<ConditionResultEvent>
    {
        private readonly EventAggregatorTests _parentClass;
        public MyHandler(EventAggregatorTests parentClass) => _parentClass = parentClass;

        public async Task Handle(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            _parentClass.MyHandlerCallCount++;
            await Task.CompletedTask;
        }
    }
 
}

