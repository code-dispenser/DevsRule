using Autofac;
using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace DevsRule.Core.Tests.Integration.Areas.Engine;

public class ConditionEngineEventHandlerDITests
{
    public ConditionEngine _conditionEngine;

    public static bool ItemInjected                 = false;
    public static bool RuleEventItemInjected        = false;
    public static bool ConditionEventItemInjected   = false;

    public ConditionEngineEventHandlerDITests()
    {
        var resolver = ConfigureAutofac();
        _conditionEngine = resolver.Resolve<ConditionEngine>();
        var injectable = resolver.Resolve<IInjectableTestItem>();
    }

    private IContainer ConfigureAutofac()
    {

        var builder = new ContainerBuilder();

        var injectableTestItem = new InjectableTestItem();
        builder.RegisterInstance(injectableTestItem).As<IInjectableTestItem>().SingleInstance();
        builder.RegisterType<RuleWithEventsRuleHandler>().As<IEventHandler<RuleResultEvent>>().InstancePerDependency();
        builder.RegisterType<RuleWithEventsConditionHandler>().As<IEventHandler<ConditionResultEvent>>().InstancePerDependency();
        builder.RegisterType<AnotherHandler>().As<IEventHandler<ConditionResultEvent>>().InstancePerDependency();
        builder.RegisterInstance(this).SingleInstance();
        builder.Register<ConditionEngine>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return new ConditionEngine(type => context.Resolve(type));
        }).SingleInstance();


        return builder.Build();
    }

    [Fact]
    public async Task Should_be_able_to_use_constructor_injection_for_registered_event_handlers()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), null);

        await _conditionEngine.EventPublisher(conditionResultEvent, CancellationToken.None, PublishMethod.WaitForAll);

        ConditionEngineEventHandlerDITests.ItemInjected.Should().BeTrue();

    }


    public class AnotherHandler : IEventHandler<ConditionResultEvent>
    {
        private readonly IInjectableTestItem _injectableTestItem;
        public AnotherHandler(IInjectableTestItem injectableTestItem)
        {
            _injectableTestItem = injectableTestItem;
        }
        public async Task Handle(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            if (_injectableTestItem is not null) ConditionEngineEventHandlerDITests.ItemInjected = true;
            await Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_handle_both_rule_level_and_condition_level_events_that_have_constructor_injection()
    {
   
        var theRule = RuleBuilder.WithName("RuleWithEvents", EventDetails.Create<RuleResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll))
                                     .ForConditionSetNamed("SetOne", "Approved")
                                         .WithPredicateCondition<Customer>("CustYears", c => c.MemberYears > 1, "Must have been a member for at least 2 years",
                                                                             EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll))
                                         .WithFailureValue("Rejected")
                                         .CreateRule();

        //await File.WriteAllTextAsync(@"D:\jsonRuleFull.json", theRule.ToJsonString(true));

        var filePath    = DataHelper.GetJsonRuleFilePath("jsonRuleFull.json");
        var jsonString  = await File.ReadAllTextAsync(filePath);

        _conditionEngine.IngestRuleFromJson(jsonString);
        
        var conditionEventSubscription  = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(InlineConditionEventHandler);
        var ruleEventSubscription       = _conditionEngine.SubscribeToEvent<RuleResultEvent>(InlineRuleEventHandler);

        var conditionEventHandledLocally = false;
        var ruleEventHandledLocally      = false;
       

        ConditionEngineEventHandlerDITests.RuleEventItemInjected     = false;
        ConditionEngineEventHandlerDITests.ConditionEventItemInjected = false;

        var theResult = await _conditionEngine.EvaluateRule("FullRule",RuleDataBuilder.AddForAny(StaticData.CustomerThree()).Create(),CancellationToken.None);
        
        //await Task.Delay(50);

        using (new AssertionScope())
        {
            ruleEventHandledLocally.Should().BeTrue();
            conditionEventHandledLocally.Should().BeTrue();
            ConditionEngineEventHandlerDITests.RuleEventItemInjected.Should().BeTrue();
            ConditionEngineEventHandlerDITests.ConditionEventItemInjected.Should().BeTrue();
        }

        async Task InlineConditionEventHandler(ConditionResultEvent conditionResultEvent, CancellationToken cancellationToken)
        {
            conditionEventHandledLocally = true;
            await Task.CompletedTask;
        }
        
        async Task InlineRuleEventHandler(RuleResultEvent ruleResultEvent, CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
            ruleEventHandledLocally = true;
            await Task.CompletedTask;
        }


    }

    public class RuleWithEventsRuleHandler : IEventHandler<RuleResultEvent>
    {
        private readonly IInjectableTestItem _injectableTestItem;
        public RuleWithEventsRuleHandler(IInjectableTestItem injectableTestItem)
            
            => _injectableTestItem = injectableTestItem;

        public async Task Handle(RuleResultEvent theEvent, CancellationToken cancellationToken)
        {
            if (_injectableTestItem is not null) ConditionEngineEventHandlerDITests.RuleEventItemInjected = true;
            await Task.CompletedTask;
        }
    }
    public class RuleWithEventsConditionHandler : IEventHandler<ConditionResultEvent>
    {
        private readonly IInjectableTestItem _injectableTestItem;
        public RuleWithEventsConditionHandler(IInjectableTestItem injectableTestItem)

            => _injectableTestItem = injectableTestItem;

        public async Task Handle(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            if (_injectableTestItem is not null) ConditionEngineEventHandlerDITests.ConditionEventItemInjected = true;
            await Task.CompletedTask;
        }
    }
}
