using DevsRule.Core.Common.Models;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Evaluators
{
    public class ConditionEvaluatorBaseTests
    {
        [Fact]
        public async Task Should_be_able_to_set_replacement_text_for_missing_properties()
        {
            var condition = new PredicateCondition<Customer>("CustomerCondition", c => c.CustomerName == "WrongName", "Replaced with @{InvalidProperty}");

            var evaluator = new TestConditionBaseEvaluator<Customer>("Missing");

            var theResult = await evaluator.Evaluate(condition, StaticData.CustomerOne(), CancellationToken.None, "101");

            theResult.FailureMessage.Should().Be("Replaced with Missing");
        }
    }
}
