using Autofac;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using Xunit;

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
    public async void DI_registered_event_handlers_should_be_able_to_be_called_using_fire_and_forget()
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
    public async void DI_registered_event_handlers_should_be_able_to_be_called_using_wait_for_all()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), null);

        await _eventAggregator.Publish(conditionResultEvent,CancellationToken.None, PublishMethod.WaitForAll);

        this.MyHandlerCallCount.Should().Be(1);
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

