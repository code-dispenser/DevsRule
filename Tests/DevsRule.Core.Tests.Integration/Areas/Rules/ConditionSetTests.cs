using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace DevsRule.Core.Tests.Integration.Areas.Rules;

public class ConditionSetTests : IClassFixture<ConditionEngineFixture>
{
    private readonly ConditionEngine _conditionEngine;
    private readonly ITestOutputHelper _outputHelper;

    public ConditionSetTests(ConditionEngineFixture fixture, ITestOutputHelper outputHelper)

        => (_conditionEngine, _outputHelper) = (fixture.ConditionEngine, outputHelper);

    [Fact]
    public async Task Should_add_serialization_exception_to_event_exception_property_if_the_condition_data_cannot_be_serialized()
    {
        var conditionSet = new ConditionSet("SetOne", new PredicateCondition<NonSerializable>("CustomerCondition", c => c.Name == "SomeValue", "Name should be SomeValue",
                                                        EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll)));
    
        var ruleData = RuleDataBuilder.AddForAny(new NonSerializable("SomeValue",42,typeof(string))).Create();

        var subscription = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(HandelTheEvent);
        
        Exception? theException = null;

        var theResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher, CancellationToken.None);

        theException.Should().NotBeNull();

        async Task HandelTheEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theException =  theEvent.SerializationException;
            await Task.CompletedTask;
        }

    }

    [Fact]
    public async Task Should_add_result_exception_to_event_exception_list_if_any_occurred()
    {
        var conditionSet = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustomerCondition", c => c.CustomerName == "CustomerName", "Name should be SomeValue",
                                                        EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll),"WrongEvaluator"));

        var ruleData = RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create();

        var subscription = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(HandelEvent);

        var exceptionListCount = 0;

        var theResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher, CancellationToken.None);


        async Task HandelEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            exceptionListCount = theEvent.ExecutionExceptions.Count;
            await Task.CompletedTask;
        }

    }
    [Fact]
    public async Task Should_raise_an_on_success_event_when_result_is_a_success_when_set_to_do_so()
    {
        var theRule = RuleBuilder.WithName("RuleOne", EventDetails.Create<RuleResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll))
                                 .ForConditionSetNamed("SetOne")
                                    .WithPredicateCondition<Customer>("CustOne", c => c.CustomerName == "CustomerOne", "Should be CustomerOne")
                                 .WithoutFailureValue()
                                 .CreateRule();

        var ruleData = RuleDataBuilder.AddForCondition("CustOne", StaticData.CustomerOne()).Create();

        var eventHandled = false;

        var subscription = _conditionEngine.SubscribeToEvent<RuleResultEvent>(HandleEvent);

        var theResult = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher);

        using (new AssertionScope())
        {
            theResult.IsSuccess.Should().BeTrue();
            eventHandled.Should().BeTrue();
        }

        async Task HandleEvent(RuleResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            eventHandled = true;
            await Task.CompletedTask;
        }
    }
    [Fact]
    public async Task Should_raise_an_event_when_result_is_a_success_and_event_is_set_for_on_success()
    {

        var conditionSet = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustOne", c => c.CustomerName == "CustomerOne", "Should be CustomerOne",
                                                                                       EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll)));

        var ruleData = RuleDataBuilder.AddForCondition("CustOne", StaticData.CustomerOne()).Create();

        var eventHandled = false;

        var subscription = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(HandleEvent);

        var theResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher,CancellationToken.None);

        using (new AssertionScope())
        {
            theResult.IsSuccess.Should().BeTrue();
            eventHandled.Should().BeTrue();
        }

        async Task HandleEvent(ConditionResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            eventHandled = true;
            await Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_raise_an_event_when_result_is_a_failure_and_the_event_is_set_for_on_failure()
    {

        var conditionSet = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustOne", c => c.CustomerName == "Wrong", "Should be CustomerOne",
                                                                                       EventDetails.Create<ConditionResultEvent>(EventWhenType.OnFailure, PublishMethod.WaitForAll)));

        var ruleData = RuleDataBuilder.AddForCondition("CustOne", StaticData.CustomerOne()).Create();

        var eventHandled = false;

        var subscription = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(HandleEvent);

        var theResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher, CancellationToken.None);

        using (new AssertionScope())
        {
            theResult.IsSuccess.Should().BeFalse();
            eventHandled.Should().BeTrue();
        }

        async Task HandleEvent(ConditionResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            eventHandled = true;
            await Task.CompletedTask;
        }
    }
}
