using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace DevsRule.Core.Tests.Integration.Areas.Evaluators;

public class PredicateConditionEvaluatorTests : IClassFixture<ConditionEngineFixture>
{
    private readonly ConditionEngine _conditionEngine;
    private readonly ITestOutputHelper _outputHelper;
    public PredicateConditionEvaluatorTests(ConditionEngineFixture conditionEngineFixture, ITestOutputHelper outputHelper)

        => (_conditionEngine, _outputHelper) = (conditionEngineFixture.ConditionEngine, outputHelper);


    [Theory]
    [InlineData("Home Town")]
    [InlineData("Some Town")]
    public async Task Should_pass_given_correct_to_evaluate_condition_and_matching_data(string town)
    {
        var customer = new Customer("CustomerOne", 1, 1, 1, new Address("AddressLine", town,"City","PostCode"));

        var rule = RuleBuilder.WithName("RuleOne")
                        .ForConditionSetNamed("SetOne")
                            .WithPredicateCondition<Customer>("ConditionOne", c => c.TotalSpend == 1M && (c.Address!.Town == "Home Town" || c.Address.Town == "Some Town"), "Total spend should be 1 with a town location of Home Town or Some Town")
                            .WithoutFailureValue()
                            .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == true);
    }

    [Theory]
    [InlineData("Home own")]
    [InlineData("Som Town")]
    public async Task Should_fail_given_correct_to_evaluate_condition_and_non_matching_data(string town)
    {
        var failureMessage = "Total spend should be 1 with a town location of Home Town or Some Town";
        var customer = new Customer("CustomerOne", 1, 1, 1, new Address("AddressLine", town, "City", "PostCode"));
 
        var rule = RuleBuilder.WithName("RuleOne")
                        .ForConditionSetNamed("SetOne")
                            .WithPredicateCondition<Customer>("ConditionOne", c => c.TotalSpend == 1M && (c.Address!.Town == "Home Town" || c.Address.Town == "Some Town"), failureMessage)
                            .WithoutFailureValue()
                            .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == false && r.FailureMessages.Count == 1 && r.EvaluationChain!.FailureMessage == failureMessage);

       
    }

    [Fact]
    public void An_evaluation_result_should_show_a_success_or_failure_and_hold_the_exception_if_one_occurred()
    {
        var theEvaluationResult = new EvaluationResult(false,"Failed message", new Exception());

        var keepCodeCoverageHappyResult = theEvaluationResult with { IsSuccess = true, Exception = null };

        using(new AssertionScope())
        {
            theEvaluationResult.Should().Match<EvaluationResult>(e => e.IsSuccess == false && e.Exception!.GetType() == typeof(Exception));
            keepCodeCoverageHappyResult.Should().Match<EvaluationResult>(e => e.IsSuccess == true && e.Exception == null);
        }

      
    }

    [Fact]
    public async Task A_predicate_condition_compilation_exception_should_be_caught_if_the_is_lambda_predicate_flag_is_set_to_false_when_it_should_be_true_for_predicate_conditions()
    {
        var jsonRule = StaticData.JsonRulePredicateMissingLambdaFlagText;

        _conditionEngine.IngestRuleFromJson(jsonRule);

        _ =  _conditionEngine.TryGetRule("RuleOneMissingLambdaFlag", out var clonedRule);

        var theLambdaFlag   = clonedRule!.ConditionSets[0].Conditions[0].IsLambdaPredicate;
        var theResult       = await _conditionEngine.EvaluateRule(clonedRule.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create());

        using(new AssertionScope())
        {
            theLambdaFlag.Should().BeFalse();
            theResult.Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions[0].GetType() == typeof(PredicateConditionCompilationException));
        }
       

    }

    [Fact]
    public async Task The_build_failure_message_in_the_base_should_return_an_empty_string_if_the_message_is_null_or_empty_which_then_gets_set_back_to_its_original_value()
    {
        //Would only happen if purposed done in a custom evaluator

        _conditionEngine.RegisterCustomEvaluator("CustomPredicateEvaluator", typeof(CustomPredicateEvaluator<>));

        ConditionSet conditionSet = new ConditionSet("SetOne", new CustomCondition<Customer>("ConditionOne","ToEvaluate","Delete In Custom Evaluator", "CustomPredicateEvaluator",new Dictionary<string, string> { ["DeleteMessage"]="true"}));

        var theResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher, CancellationToken.None);

        theResult.FailureMessage.Should().Be("Delete In Custom Evaluator");


    }
}
