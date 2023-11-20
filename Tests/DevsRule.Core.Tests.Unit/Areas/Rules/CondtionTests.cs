using DevsRule.Core.Areas.Rules;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class CondtionTests
{
    [Theory]
    [InlineData("","ToEvaluate","failureMessage","EvaluatorTypeName")]
    [InlineData("ConditionName", "", "failureMessage", "EvaluatorTypeName")]
    [InlineData("ConditionName", "ToEvaluate", "", "EvaluatorTypeName")]
    [InlineData("ConditionName", "ToEvaluate", "failureMessage", "")]
    public void Should_throw_argument_exeption_exception_if_condition_name_is_null_empty_whitespace(
        string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName)
    {
        FluentActions.Invoking(() => new Condition<Customer>(conditionName, toEvaluate,failureMessage, evaluatorTypeName, false,null, null)).Should()
            .Throw<ArgumentException>();

    }

    [Fact]
    public void Should_trim_condtion_name_and_evaluator_type_name()
    {
        var theCondition = new Condition<Customer>("  ConditionName   ","ToEvaluator","FailureMessage", "  EvaluatorTypeName   ", false, null, null);

        theCondition.Should().Match<Condition<Customer>>(c => c.ConditionName == "ConditionName" && c.EvaluatorTypeName == "EvaluatorTypeName");
    }


}
