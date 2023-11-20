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
using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Engine;

public class ConditionEngineTests
{
    [Fact]
    public async Task Should_throw_a_rule_not_found_exception_using_the_find_and_evaluate_method_if_the_rule_is_not_in_cache()

        => await FluentActions.Invoking(() => new ConditionEngine().EvaluateRule("MissingRule", new RuleData(new[] {new DataContext(StaticData.CustomerOne())} ))).Should().ThrowExactlyAsync<RuleNotFoundException>();


    [Fact]
    public void Should_throw_a_rule_from_json_exception_if_a_rule_cannot_be_created_from_the_json_string()

        => FluentActions.Invoking(() => new ConditionEngine().RuleFromJson("not a rule")).Should().ThrowExactly<RuleFromJsonException>();

    [Fact]
    public void The_rule_from_json_method_should_throw_a_rule_from_json_exception_with_an_inner_event_not_found_if_there_is_the_condition_event_details_are_incorrect()
    {
        var jsonString = RuleBuilder.WithName("EvenRule")
                                      .ForConditionSetNamed("EventSet")
                                      .WithPredicateCondition<Customer>("CustomerCondition", c => c.CustomerName == "CustomerOne", "Should be called Customerone",
                                            EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.FireAndForget))
                                      .WithoutFailureValue()
                                      .CreateRule()
                                      .ToJsonString();
        jsonString = jsonString.Replace("ConditionResultEvent", "ResultConditionEvent");

        FluentActions.Invoking(() => new ConditionEngine().RuleFromJson(jsonString))
                     .Should().ThrowExactly<RuleFromJsonException>().WithInnerExceptionExactly<EventNotFoundException>();
    }
    [Fact]
    public void The_rule_from_json_method_should_throw_a_rule_from_json_exception_with_an_inner_event_not_found_if_there_is_the_rule_event_details_are_incorrect()
    {
        var jsonString = RuleBuilder.WithName("EvenRule", EventDetails.Create<RuleResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.FireAndForget))
                                      .ForConditionSetNamed("EventSet")
                                      .WithPredicateCondition<Customer>("CustomerCondition", c => c.CustomerName == "CustomerOne", "Should be called Customerone")
                                           
                                      .WithoutFailureValue()
                                      .CreateRule()
                                      .ToJsonString();
        jsonString = jsonString.Replace("RuleResultEvent", "ResultRuleEvent");

        FluentActions.Invoking(() => new ConditionEngine().RuleFromJson(jsonString))
                     .Should().ThrowExactly<RuleFromJsonException>().WithInnerExceptionExactly<EventNotFoundException>();
    }

    [Fact]
    public void Should_throw_a_missing_condition_to_evaluate_property_value_exception_if_the_json_cannot_be_converted_due_to_the_missing_to_evaluate_value()
    {
        JsonRule jsonRule = JsonSerializer.Deserialize<JsonRule>(StaticData.JsonRuleText)!;

        jsonRule.ConditionSets[0].Conditions[0].ToEvaluate = String.Empty;

        string jsonText = JsonSerializer.Serialize<JsonRule>(jsonRule);
        
        FluentActions.Invoking(() => new ConditionEngine().RuleFromJson(jsonText)).Should().ThrowExactly<MissingConditionToEvaluatePropertyValue>();

    }

    [Fact]
    public void Should_throw_a_context_type_assembly_not_found_exception_if_the_context_type_is_not_listed_in_the_types_in_the_app_doamin_assemblies()
    {
        JsonRule jsonRule = JsonSerializer.Deserialize<JsonRule>(StaticData.JsonRuleText)!;

        jsonRule.ConditionSets[0].Conditions[0].ContextTypeName = "Bogus.Context.Type";

        string jsonText = JsonSerializer.Serialize<JsonRule>(jsonRule);

        FluentActions.Invoking(() => new ConditionEngine().RuleFromJson(jsonText)).Should().ThrowExactly<ContextTypeAssemblyNotFound>();

    }

    [Fact]
    public void Should_be_able_to_add_a_handcrafted_rule_to_the_condition_engine()
    {
        var conditionEngine = new ConditionEngine();

        var theRule = RuleBuilder
                        .WithName("RuleOne")
                            .ForConditionSetNamed("SetOne")
                                .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne")
                            .OrConditionSetNamed("SetTwo")
                                .WithPredicateCondition<Customer>("CustNo", c => c.CustomerNo == 111, "Customer No. should be 111")
                            .WithFailureValue(String.Empty)
                        .CreateRule();

        conditionEngine.AddOrUpdateRule(theRule);

        _ = conditionEngine.TryGetRule(theRule.RuleName, out var ruleInEngine);

        ruleInEngine.Should().NotBeNull().And.Match<Rule>(r => r.RuleName == theRule.RuleName && r.ConditionSets.Count == 2);
    }

    [Fact]
    public void Should_be_able_to_get_an_added_rule_from_the_condition_engine()
    {
        var conditionEngine = new ConditionEngine();

        JsonRule jsonRule = JsonSerializer.Deserialize<JsonRule>(StaticData.JsonRuleText)!;

        string jsonText = JsonSerializer.Serialize<JsonRule>(jsonRule);

        conditionEngine.IngestRuleFromJson(jsonText);

        _ = conditionEngine.TryGetRule(jsonRule.RuleName!, out var ruleInEngine);

        ruleInEngine.Should().NotBeNull().And.Match<Rule>(r => r.RuleName == jsonRule.RuleName && r.ConditionSets.Count == 1);
    }

    [Fact]
    public void Trying_to_get_a_non_existent_rule_from_the_condition_engine_Should_return_false()
    
        => new ConditionEngine().TryGetRule("NonExistentRule", out _).Should().BeFalse();

    [Fact]
    public void Should_be_able_to_remove_an_added_rule_from_the_condition_engine()
    {
        var conditionEngine = new ConditionEngine();

        var theRule = RuleBuilder
                        .WithName("RuleOne")
                            .ForConditionSetNamed("SetOne")
                                .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne")
                            .OrConditionSetNamed("SetTwo")
                                .WithPredicateCondition<Customer>("CustNo", c => c.CustomerNo == 111, "Customer No. should be 111")
                            .WithFailureValue("10")
                        .CreateRule();

        conditionEngine.AddOrUpdateRule(theRule);

        _ = conditionEngine.TryGetRule(theRule.RuleName, out var ruleInEngine);

        conditionEngine.RemoveRule(theRule.RuleName);

        var isRuleStillInEngine = conditionEngine.TryGetRule(theRule.RuleName, out _);

        using(new AssertionScope())
        {
            ruleInEngine.Should().NotBeNull().And.Match<Rule>(r => r.RuleName == theRule.RuleName && r.ConditionSets.Count == 2);

            isRuleStillInEngine.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Should_be_able_register_and_get_an_instance_of_a_custom_evaluator_when_requested()
    {
        var conditionEngine = new ConditionEngine();
        
        conditionEngine.RegisterCustomEvaluator("FixedContextConditionEvaluator", typeof(CustomerOnlyEvaluator));

        var customEvaluatorInstance = (CustomerOnlyEvaluator)conditionEngine.GetEvaluatorByName("FixedContextConditionEvaluator", typeof(CustomerOnlyEvaluator));

        var condition = new CustomCondition<Customer>("Custom", "some val", "should be . . .", "FixedContextConditionEvaluator");

        var theEvaluationResult = await customEvaluatorInstance.Evaluate(condition, StaticData.CustomerOne(),CancellationToken.None,GlobalStrings.Default_TenantID);

        theEvaluationResult.Should().NotBeNull();
    }

    [Fact]
    public void Should_replace_null_tenantid_and_cultureid_with_defaults_when_using_add_or_update_rule()
    {
        var conditionEngine = new ConditionEngine();
        var rule            = new Rule("RuleOne");

        conditionEngine.AddOrUpdateRule(rule, null, null);

        var foundRule = conditionEngine.ContainsRule("RuleOne", "All_Tenants", "en-GB");

        foundRule.Should().BeTrue();    
    }

}
