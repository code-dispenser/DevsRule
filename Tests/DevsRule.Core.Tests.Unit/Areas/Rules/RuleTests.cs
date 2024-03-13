using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class RuleTests : IClassFixture<ConditionEngineFixture>
{
    private readonly ConditionEngine    _conditionEngine;
    private readonly ITestOutputHelper  _outputHelper;

    public RuleTests(ConditionEngineFixture fixture, ITestOutputHelper outputHelper)
        
        => (_conditionEngine, _outputHelper) = (fixture.ConditionEngine, outputHelper);


    [Fact]
    public void A_rule_should_set_its_tenant_id_and_culture_id_properties_to_the_defaults_if_empty_or_missing()

        => new Rule("RuleOne","",null,"","")
                .Should().Match<Rule>(r => r.TenantID == GlobalStrings.Default_TenantID && r.CultureID == GlobalStrings.Default_CultureID);
    [Fact]
    public void A_rule_should_set_its_tenant_id_and_culture_id_properties_to_the_defaults_if_empty_or_missing_when_a_condition_set_is_also_part_of_the_constructor()

        => new Rule("RuleOne", new ConditionSet("SetOne"), "",  null, "", "")
            .Should().Match<Rule>(r => r.TenantID == GlobalStrings.Default_TenantID && r.CultureID == GlobalStrings.Default_CultureID);

    [Fact]
    public async Task Evaluating_a_rule_without_condition_sets_should_return_a_failed_result_with_a_missing_condition_sets_exception_in_its_exception_list()

        =>  (await new Rule("RuleOne").Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher))
                            .Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(MissingConditionSetsException));

    [Fact]
    public async Task Evaluating_a_rule_without_contexts_should_return_a_failed_result_with_a_missing_rule_contexts_exception_its_exception_list()

    => (await new Rule("RuleOne").Evaluate(_conditionEngine.GetEvaluatorByName, null!, _conditionEngine.EventPublisher))
                        .Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(MissingRuleContextsException));

    [Fact]
    public async Task Evaluating_a_rule_with_null_contexts_should_return_a_failed_result_with_a_missing_rule_contexts_exception_its_exception_list()

        => (await new Rule("RuleOne").Evaluate(_conditionEngine.GetEvaluatorByName, null!, _conditionEngine.EventPublisher))
                            .Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(MissingRuleContextsException));

    [Fact]
    public async Task Evaluating_a_rule_with_an_empty_context_array_should_return_a_failed_result_with_a_missing_rule_contexts_exception_its_exception_list()

    => (await new Rule("RuleOne").Evaluate(_conditionEngine.GetEvaluatorByName, new RuleData(null!), _conditionEngine.EventPublisher))
                        .Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(MissingRuleContextsException));

    [Fact]
    public async Task Evaluating_a_rule_with_a_condition_set_with_no_conditions_should_return_a_failed_result_with_a_missing_conditions_exception_in_its_exception_list()

        => (await new Rule("RuleOne", new ConditionSet("ConditionSetOne"))                                
                .Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher))
                                .Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(MissingConditionsException));


    [Fact]
    public async Task A_disabled_rule_should_not_evaluate_any_conditions_sets_and_return_a_failed_rule_result_with_the_rule_disabled_flag_set_to_true()
    {
        var theRule = new Rule("RuleOne", new ConditionSet("ConditionSetOne"));
        
        theRule.IsEnabled = false;

        (await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher))
                            .Should().Match<RuleResult>(r => r.RuleDisabled == true && r.IsSuccess == false && r.FailureMessages.Count == 0 
                                                        && r.Exceptions.Count == 0 && r.TotalEvaluations == 0);
                            
    }
    [Fact]
    public void The_rule_to_json_rule_method_should_return_a_valid_json_rule_object_with_matching_values()
    {
        var additionalItems = new Dictionary<string, string> { ["One"] = "1", ["Two"] = "2" };
        var conditionOne    = new CustomCondition<Customer>
            ("ConditionOne", c => c.CustomerName == "CustomerOne", 
            "Customer Name should be CustomerOne", "PredicateConditionEvaluator", additionalItems,
             EventDetails.Create<ConditionResultEvent>());
        
        var conditionSetOne = new ConditionSet("ConditionSetOne", conditionOne, "SetValue");

        var theRule = new Rule("RuleOne", conditionSetOne, "FailureValue", EventDetails.Create<RuleResultEvent>(), "TenantID", "CultureID");
        
        var theJsonRule = theRule.RuleToJsonRule();

        //AssertionOptions.FormattingOptions.MaxDepth = 10;

        theJsonRule.Should().Match<JsonRule>(j => j.RuleName == theRule.RuleName && j.FailureValue == theRule.FailureValue && j.TenantID == theRule.TenantID && j.CultureID == theRule.CultureID
                                            && j.RuleEventDetails!.PublishMethod == PublishMethod.FireAndForget.ToString()
                                            && j.RuleEventDetails.EventWhenType == EventWhenType.OnSuccessOrFailure.ToString()
                                            && j.ConditionSets[0].SetValue == conditionSetOne.SetValue && j.ConditionSets[0].ConditionSetName == conditionSetOne.ConditionSetName
                                            && j.ConditionSets[0].Conditions[0].FailureMessage     == conditionOne.FailureMessage
                                            && j.ConditionSets[0].Conditions[0].ToEvaluate         == "c => (c.CustomerName == \"CustomerOne\")"
                                            && j.ConditionSets[0].Conditions[0].ContextTypeName    == "DevsRule.Tests.SharedDataAndFixtures.Models.Customer"
                                            && j.ConditionSets[0].Conditions[0].ConditionName      == conditionOne.ConditionName
                                            && j.ConditionSets[0].Conditions[0].EvaluatorTypeName  == "PredicateConditionEvaluator"
                                            && j.ConditionSets[0].Conditions[0].AdditionalInfo["One"] == "1"
                                            && j.ConditionSets[0].Conditions[0].AdditionalInfo["Two"] == "2"
                                            && j.ConditionSets[0].Conditions[0].ConditionEventDetails!.EventWhenType == EventWhenType.OnSuccessOrFailure.ToString()
                                            && j.ConditionSets[0].Conditions[0].ConditionEventDetails!.PublishMethod == PublishMethod.FireAndForget.ToString());


    }

    [Fact]
    public void The_rule_to_json_string_should_return_a_valid_json_string_with_the_correct_values()
    {
        var additionalItems = new Dictionary<string, string> { ["One"] = "1", ["Two"] = "2" };
        var conditionOne    = new CustomCondition<Customer>("ConditionOne", "c => c.CustomerName == \"CustomerName\"", 
            "Customer Name should be CustomerOne", "SomeConditionEvaluator", additionalItems,
            EventDetails.Create<ConditionResultEvent>());

        var conditionSetOne = new ConditionSet("ConditionSetOne", conditionOne, "SetValue");

        var theRule = new Rule("RuleOne", conditionSetOne, "FailureValue", EventDetails.Create<RuleResultEvent>(), "TenantID", "CultureID");

        var jsonString = theRule.ToJsonString();

        var theRuleFromString       = _conditionEngine.RuleFromJson(jsonString);
        var theFromStringCondition  = theRuleFromString.ConditionSets[0].Conditions[0];

        using (new AssertionScope())
        {
            theRuleFromString.Should().BeEquivalentTo(theRule, options => options.Excluding(c => c.ConditionSets));

            theRuleFromString.ConditionSets[0].Should().BeEquivalentTo(conditionSetOne, options => options.Excluding(c => c.Conditions));

            ((Condition<Customer>)theFromStringCondition).Should().BeEquivalentTo(conditionOne, options => options.Excluding(c => c.CompiledPredicate));
        }
    }

    [Fact]
    public void The_method_deep_clone_rule_should_make_a_proper_unreferenced_copy()
    {
        var additionalItems = new Dictionary<string, string> { ["One"] = "1", ["Two"] = "2" };
        var conditionOne    = new PredicateCondition<Customer>("ConditionOne", c => c.CustomerName == "CustomerName", 
                                                                "Customer Name should be CustomerOne", "PredicateConditionEvaluator", additionalItems,
                                                                EventDetails.Create<ConditionResultEvent>());

        var conditionSetOne = new ConditionSet("ConditionSetOne", conditionOne, "SetValue");

        var theRule = new Rule("RuleOne", conditionSetOne, "FailureValue", EventDetails.Create<RuleResultEvent>(),  "TenantID", "CultureID");

        var clonedRule = Rule.DeepCloneRule(theRule);

        clonedRule.OrConditionSet(conditionSetOne);

        clonedRule.ConditionSets.First().Conditions[0].AdditionalInfo["One"] = "Two";

        
        using (new AssertionScope())
        {
            theRule.ConditionSets.Count.Should().Be(1);
            theRule.ConditionSets.First().Conditions[0].AdditionalInfo["One"].Should().Be("1");
            
            clonedRule.ConditionSets.First().Conditions[0].AdditionalInfo["One"].Should().Be("Two");

        }

    }
    [Fact]
    public void The_method_deep_clone_rule_should_make_an_identical_copy()
    {
        var additionalItems = new Dictionary<string, string> { ["One"] = "1", ["Two"] = "2" };
        var conditionOne    = new PredicateCondition<Customer>("ConditionOne", c => c.CustomerName == "CustomerName", 
                                                            "Customer Name should be CustomerOne", "PredicateConditionEvaluator", additionalItems,
                                                            EventDetails.Create<ConditionResultEvent>());

        var conditionSetOne = new ConditionSet("ConditionSetOne", conditionOne, "SetValue");

        var theRule = new Rule("RuleOne", conditionSetOne, "FailureValue", EventDetails.Create<RuleResultEvent>(), "TenantID", "CultureID");

        var clonedRule = Rule.DeepCloneRule(theRule);

        using (new AssertionScope())
        {
            clonedRule.Should().BeEquivalentTo(theRule, options => options.Excluding(c => c.ConditionSets));

            clonedRule.ConditionSets[0].Should().BeEquivalentTo(conditionSetOne, options => options.Excluding(c => c.Conditions));

            ((Condition<Customer>)clonedRule.ConditionSets[0].Conditions[0]).Should().BeEquivalentTo(conditionOne, options => options.Excluding(c => c.CompiledPredicate));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void The_rule_to_json_string_method_should_not_escape_characters_when_set_to_false(bool useEscaped)
    {
        var conditionSet = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustomerCondition", c => c.CustomerName == "CustomerOne", "Should be CustomerOne"));
        var theRule     = new Rule("RuleOne", conditionSet, "FailureValue");

        var theJsonString = theRule.ToJsonString(false, useEscaped);

        if (true  == useEscaped) theJsonString.Should().NotContain("=>");
        if (false == useEscaped) theJsonString.Should().Contain("=>");

    }
}
