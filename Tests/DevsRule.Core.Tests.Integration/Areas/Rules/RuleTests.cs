using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace DevsRule.Core.Tests.Integration.Areas.Rules;

public class RuleTests :IClassFixture<ConditionEngineDIFixture>
{
    private readonly ConditionEngine    _conditionEngine;
    private readonly ITestOutputHelper  _outputHelper;

    public RuleTests(ConditionEngineDIFixture fixture, ITestOutputHelper outputHelper)

        => (_conditionEngine, _outputHelper) = (fixture.ConditionEngine, outputHelper);


    [Fact]
    public async Task Should_be_able_to_cancel_a_long_running_evaluation()
    {
        var additionalItems = new Dictionary<string, string> { ["One"] = "1", ["Two"] = "2" };

        var ruleOne = RuleBuilder.WithName("RuleOne")
                                    .ForConditionSetNamed("SetOne")
                                        .WithCustomPredicateCondition<Customer>("ConditionOne", c => c.CustomerName == "CustomerName", "Customer Name should be CustomerOne", "CustomDIRequiredEvaluator", additionalItems)
                                    .WithoutFailureValue()
                                    .CreateRule();

        var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                    .ForConditionSetNamed("SetTwo")
                                        .WithPredicateCondition<Customer>("CustomerNo", c => c.CustomerNo == 1, "Customer number should be 1")
                                    .WithoutFailureValue()
                                    .CreateRule();


        _conditionEngine.AddOrUpdateRule(ruleOne);
        _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("CustomDIRequiredEvaluator", typeof(CustomDIRequiredEvaluator<>));

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        tokenSource.Cancel();

        var theResultOne = await _conditionEngine.EvaluateRule(ruleOne.RuleName,RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), tokenSource.Token);

        _conditionEngine.AddOrUpdateRule(ruleTwo);

        var theResultTwo = await _conditionEngine.EvaluateRule(ruleTwo.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create());

        using(new AssertionScope())
        {
            theResultOne.Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(OperationCanceledException));
            theResultTwo.Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions.Count == 0);
        }
      
    }


    [Fact]
    public async Task Should_be_able_to_create_a_rule_output_to_json_then_ingest_the_json_and_evaluate_it()
    {
        
        var theRule = RuleBuilder
                        .WithName("RuleOne")
                            .ForConditionSetNamed("SetOne")
                                .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne")
                                .AndPredicateCondition<Supplier>("SupName", s => s.SupplierName == "Supplier name", "should be SupplierOne")
                                .AndCustomPredicateCondition<Customer>("CustYears", c => c.MemberYears > 1, "Member years should be greater than 1", "CustomPredicteEvaluator")
                            .OrConditionSetNamed("SetTwo")
                                .WithCustomPredicateCondition<Customer>("CustNo", c => c.CustomerNo == 111, "Customer No. should be 111", "CustomPredicteEvaluator")
                                .AndRegexCondition<Customer>("CustAddressLine", c => c.Address!.AddressLine, "^CustomerOne$", "AddressLine should be something", RegexOptions.None)
                            .OrConditionSetNamed("SetThree")
                                .WithCustomCondition<Supplier>("SupCustom", "some value", "Should be some value", "CustomEvaluatorName", new Dictionary<string, string>())
                                .AndCustomCondition<Customer>("CustYearsOne", "2", "Customer years should be 2", "CustomEvaluatorName", new Dictionary<string, string>() { ["One"]="1" })
                                .AndCustomCondition<Customer>("CustYearsTwo", "2", "Customer years should be 2", "CustomEvaluatorName", new Dictionary<string, string>() { ["Two"]="2" })
                            .WithFailureValue("10")
                        .CreateRule();


        _conditionEngine.IngestRuleFromJson(theRule.ToJsonString());

        //_conditionEngine.AddRule(theRule);
        _conditionEngine.RegisterCustomEvaluator("CustomPredicteEvaluator", typeof(CustomPredicteEvaluator<>));

        //CustomEvaluatorName in set three has not been registered so it should create an exception.

        var contexts = RuleDataBuilder.AddForCondition("CustName", StaticData.CustomerOne())
                                     .AndForCondition("CustYearsOne", StaticData.CustomerOne())
                                     .AndForCondition("CustYears", StaticData.CustomerTwo())
                                     .AndForCondition("CustYearsTwo", StaticData.CustomerTwo())
                                     .AndForAny(StaticData.SupplierOne()).Create();

        var theConditionCount = theRule.ConditionSets[0].Conditions.Count() + theRule.ConditionSets[1].Conditions.Count() + theRule.ConditionSets[2].Conditions.Count();
        var theResult        = await _conditionEngine.EvaluateRule("RuleOne", contexts);
       
        using(new AssertionScope())
        {
            theConditionCount.Should().Be(8);
            theResult.Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions.Count == 1 && r.FailureMessages.Count == 3 && r.TotalEvaluations == 5);
        }

        
    }

   [Fact]
   public async Task Rules_with_events_at_the_rule_level_should_trigger_as_of_the_event_when_type()
   {
        RuleResultEvent theRuleResultEvent = default!;

        var theRule = RuleBuilder.WithName("RuleWithEvent", EventDetails.Create<RuleResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll))
                                    .ForConditionSetNamed("SetOne","Approved")
                                        .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should be CustomerOne")
                                    .WithFailureValue("Rejected")
                                    .CreateRule();


        _= _conditionEngine.SubscribeToEvent<RuleResultEvent>(EventHandler);
        _conditionEngine.AddOrUpdateRule(theRule);

        var theRuleResult = await _conditionEngine.EvaluateRule(theRule.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create("TestTenantID"));

        async Task EventHandler(RuleResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            theRuleResultEvent = ruleEvent;
            await Task.CompletedTask;
        }

        theRuleResultEvent.Should().Match<RuleResultEvent>(r => r.IsSuccessEvent == true && r.SuccessValue == "Approved" && r.FailureValue == "Rejected" 
                                                        && r.ExecutionExceptions.Count == 0 && r.TenantID == "TestTenantID");
   }

    [Fact]
    public async Task Rules_with_events_at_both_the_rule_and_condition_level_should_trigger_as_of_the_event_when_type()
    {
        RuleResultEvent         theRuleResultEvent  = default!;
        ConditionResultEvent    theConditionEvent    = default!;

        var theRule = RuleBuilder.WithName("RuleWithEvent", EventDetails.Create<RuleResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll))
                                    .ForConditionSetNamed("SetOne", "Approved")
                                        .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should be CustomerOne",
                                            EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll))
                                    .WithFailureValue("Rejected")
                                    .CreateRule();


        _= _conditionEngine.SubscribeToEvent<RuleResultEvent>(RuleEventHandler);
        _= _conditionEngine.SubscribeToEvent<ConditionResultEvent>(ConditionEventHandler);
        _conditionEngine.AddOrUpdateRule(theRule);

        var theRuleResult = await _conditionEngine.EvaluateRule(theRule.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create("TestTenantID"));

        async Task RuleEventHandler(RuleResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            theRuleResultEvent = ruleEvent;
            await Task.CompletedTask;
        }

        async Task ConditionEventHandler(ConditionResultEvent conditionEvent, CancellationToken cancellationToken)
        {
            theConditionEvent = conditionEvent;
            await Task.CompletedTask;
        }

        using (new AssertionScope())
        {
            theRuleResultEvent.Should().Match<RuleResultEvent>(r => r.IsSuccessEvent == true && r.SuccessValue == "Approved" && r.FailureValue == "Rejected"
                                                            && r.ExecutionExceptions.Count == 0 && r.TenantID == "TestTenantID" && r.SenderName == theRule.RuleName);

            _= theConditionEvent.TryGetData(out var contextData);

            theConditionEvent.Should().Match<ConditionResultEvent>(c => c.SenderName == theRule.ConditionSets[0].Conditions[0].ConditionName && c.IsSuccessEvent == true
                                                                    && c.ContextType == typeof(Customer) && c.ExecutionExceptions.Count == 0 && c.SerializationException == null
                                                                    && c.TenantID == "TestTenantID" && contextData != null);
        }

    }

    [Fact]
    public async Task The_rule_should_return_a_failed_rule_result_with_an_missing_rule_contexts_exception_for_un_matched_condition_data_contexts()
    {
        var theRule = RuleBuilder.WithName("RuleOne")
                            .ForConditionSetNamed("ConditionOne")
                                .WithPredicateCondition<Customer>("CustomerName", c => c.CustomerName == "CustomerOne", "The name should be CustomerOne")
                                .WithoutFailureValue()
                                .CreateRule();

        var ruleData = RuleDataBuilder.AddForCondition("WrongName", StaticData.CustomerOne()).Create();

        var theResult = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher);

        theResult.Exceptions[0].Should().BeOfType<MissingRuleContextsException>();
    }

    [Fact]
    public async Task Should_raise_a_failed_event_with_a_failing_result_when_set_to_do_so()
    {
        var theRule = RuleBuilder.WithName("RuleOne", EventDetails.Create<RuleResultEvent>(EventWhenType.OnFailure, PublishMethod.WaitForAll))
                                 .ForConditionSetNamed("SetOne")
                                    .WithPredicateCondition<Customer>("CustOne", c => c.CustomerName == "WrongName", "Should be CustomerOne")
                                 .WithoutFailureValue()
                                 .CreateRule();

        var ruleData = RuleDataBuilder.AddForCondition("CustOne",StaticData.CustomerOne()).Create();
        
        var eventHandled = false;

        var subscription = _conditionEngine.SubscribeToEvent<RuleResultEvent>(HandleEvent);

        var theResult = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName,ruleData, _conditionEngine.EventPublisher);

        using(new AssertionScope())
        {
            theResult.IsSuccess.Should().BeFalse();
            eventHandled.Should().BeTrue();
        }

        async Task HandleEvent(RuleResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            eventHandled = true;
            await Task.CompletedTask;
        }
    }
    [Fact]
    public async Task Should_raise_an_event_with_any_condition_exceptions_added_to_the_events_exception_list()
    {
        var condtionSet = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustomerCondition", c => c.Address!.AddressLine == "Some Street", "Should be some street", "IncorrectEvaluator"));
        var theRule     = new Rule("RuleOne", condtionSet,"", EventDetails.Create<RuleResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll));

        var customer = new Customer("Customer", 1, 1, 1, new Address("Some Street", "Some Town", "Some City", "Some Post Code"));
        var ruleData = RuleDataBuilder.AddForCondition("CustomerCondition", customer).Create();

        var theExceptionListCount = 0;

        var subscription = _conditionEngine.SubscribeToEvent<RuleResultEvent>(HandleEvent);
        var theResult    = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher);

        using (new AssertionScope())
        {
            theResult.IsSuccess.Should().BeFalse();
            theExceptionListCount.Should().Be(1);
        }

        async Task HandleEvent(RuleResultEvent ruleEvent, CancellationToken cancellationToken)
        {
            theExceptionListCount = ruleEvent.ExecutionExceptions.Count;
            await Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Rule_should_end_execution_if_a_condition_set_conditions_have_errors_somewhere_in_its_evaluation_chain()
    {
        var condtionSetOne = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustomerCondition", c => c.Address!.AddressLine == "Not Some Street", "Should be some street"));
        var conditionSetTwo = new ConditionSet("SetTwo", new PredicateCondition<Customer>("HasError", c => c.CustomerName == "CustomerOne", "Customer name should be CustomerOne", "IncorrectEvaluatorTypeName"));

        var theRule = new Rule("RuleOne", condtionSetOne).OrConditionSet(conditionSetTwo);

        var customer = new Customer("Customer", 1, 1, 1, new Address("Some Street", "Some Town", "Some City", "Some Post Code"));
        var ruleData = RuleDataBuilder.AddForCondition("CustomerCondition", customer).Create();

        var theResult = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, ruleData, _conditionEngine.EventPublisher);

        using (new AssertionScope())
        {
            theResult.IsSuccess.Should().BeFalse();
            theResult.Exceptions.Should().HaveCount(1);
        }

    }


}
