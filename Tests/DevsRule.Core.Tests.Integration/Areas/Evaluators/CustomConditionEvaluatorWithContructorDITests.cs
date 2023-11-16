using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Integration.Areas.Evaluators;

public class CustomConditionEvaluatorWithContructorDITests : IClassFixture<ConditionEngineDIFixture>
{

    private readonly ConditionEngine _conditionEngine;
    public CustomConditionEvaluatorWithContructorDITests(ConditionEngineDIFixture conditionEngineDIFixture)

        => _conditionEngine = conditionEngineDIFixture.ConditionEngine;
    [Fact]
    public async Task Shoud_be_able_to_use_constructor_injection_with_custom_evaluators_regisitered_in_a_di_container()
    {
        var theRule = RuleBuilder.WithName("RuleUsingDIEvaluator")
                                    .ForConditionSetNamed("DISet")
                                        .WithCustomPredicateCondition<Customer>("CustomDICondition", c => c.CustomerName == "Wrong" || c.CustomerNo == 111, "Should have correct name or number to pass test", "CustomDIRequiredEvaluator")
                                    .WithoutFailureValue()
                                    .CreateRule();

       _conditionEngine.IngestRuleFromJson(theRule.ToJsonString());
            
       _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("CustomDIRequiredEvaluator", typeof(CustomDIRequiredEvaluator<>));

        var theResult = await _conditionEngine.EvaluateRule("RuleUsingDIEvaluator", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create());

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == true && r.Exceptions.Count == 0);
                                    
    }
}
