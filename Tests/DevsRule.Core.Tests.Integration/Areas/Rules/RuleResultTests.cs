using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DevsRule.Core.Tests.Integration.Areas.Rules;

public class RuleResultTests : IClassFixture<ConditionEngineFixture>
{
    private readonly ConditionEngine _conditionEngine;
    private readonly ITestOutputHelper _outputHelper;

    public RuleResultTests(ConditionEngineFixture fixture, ITestOutputHelper outputHelper)

        => (_conditionEngine, _outputHelper) = (fixture.ConditionEngine, outputHelper);

    [Fact]
    public async Task The_rule_result_should_contain_the_correct_number_of_condition_evaluations()
    {
        var theRule = RuleBuilder
                        .WithName("RuleOne")
                            .ForConditionSetNamed("SetOne")
                                .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne")
                                .AndPredicateCondition<Supplier>("SupName", s => s.SupplierName == "bad name", "should be SupplierOne")
                            .OrConditionSetNamed("SetTwo")
                                .WithPredicateCondition<Customer>("CustNo", c => c.CustomerNo == 111, "Customer No. should be 111")
                            .WithoutFailureValue()
                        .CreateRule();
        /*
            * The first condition passes so needs to be And'ed with the second condition
            * The second condition fails which fails the set so the rule then needs to do an Or between the condition sets
            * The third condition is evaluated and passes. The rule is a success with three evaluations
        */

        var contexts = RuleDataBuilder.AddForAny(StaticData.CustomerOne()).AndForAny(StaticData.SupplierOne()).Create();

        var theResult = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, contexts, _conditionEngine.EventPublisher);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess && r.TotalEvaluations == 3);
    }

    [Fact]
    public async Task The_rule_result_should_contain_the_name_of_the_rule_evalueated_and_the_total_time_taken_in_both_milli_and_microseconds()
    {
        var theRule = RuleBuilder
                        .WithName("RuleOne")
                            .ForConditionSetNamed("SetOne")
                                .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne")
                                .AndPredicateCondition<Supplier>("SupName", s => s.SupplierName == "bad name", "should be SupplierOne")
                            .OrConditionSetNamed("SetTwo")
                                .WithPredicateCondition<Customer>("CustNo", c => c.CustomerNo == 111, "Customer No. should be 111")
                            .WithFailureValue("5")
                        .CreateRule();
        /*
            * The first condition passes, but needs to be And'ed with the second condition
            * The second condition fails so the rule needs to do an Or between the condition sets
            * The third condition is evaluated an passes. The rule is a success with three evaluations
        */

        var contexts = RuleDataBuilder.AddForAny(StaticData.CustomerOne()).AndForAny(StaticData.SupplierOne()).Create();

        var theResult = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, contexts, _conditionEngine.EventPublisher);

        theResult.Should().Match<RuleResult>(r => r.RuleName == theRule.RuleName && r.RuleTimeMilliseconds == r.RuleTimeMicroseconds / 1000);
    }

}
