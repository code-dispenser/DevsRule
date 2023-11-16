using DevsRule.Core.Areas.Rules;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class CustomConditionTests
{
    [Fact]
    public void Should_throw_argument_null_exception_if_the_predicate_condition_is_null()

       => FluentActions.Invoking(() => new CustomCondition<Customer>("Some condition",predicateExpression: null!, "Failed","SomeEvaluator"))
                           .Should().ThrowExactly<ArgumentNullException>();

    [Fact()]
    public void The_type_name_should_be_populated_given_a_context_and_a_condition()

        => new CustomCondition<Customer>("Customer One", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message,"SomeEvaluator")
                        .ContextType.FullName.Should().Be("DevsRule.Tests.SharedDataAndFixtures.Models.Customer");

    [Fact]
    public void The_to_evaluate_string_should_be_populated_with_a_string_representaion_of_the_condition()
    {
        Expression<Func<Customer, bool>> condition = (c) => c.CustomerName == "CustomerOne";

        new CustomCondition<Customer>("Customer One",condition, StaticData.Customer_One_Name_Message, "SomeEvaluator")
                .ToEvaluate
                    .Should().Be(condition.ToString());
    }

    [Fact]
    public void The_condition_should_have_a_working_compiled_predicate_if_a_predicate_expression_is_used()
    {
        var condition = new CustomCondition<Customer>("Customer One", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message, "SomeEvaluator");

        var compiledFunc = condition.CompiledPrediate!;

        var thePredicateResult = compiledFunc(StaticData.CustomerOne());

        thePredicateResult.Should().BeTrue();

    }
}
