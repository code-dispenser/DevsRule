using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
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
    public void The_to_evaluate_string_should_be_populated_with_a_string_representation_of_the_condition()
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

        var compiledFunc = condition.CompiledPredicate!;

        var thePredicateResult = compiledFunc(StaticData.CustomerOne());

        thePredicateResult.Should().BeTrue();

    }

    [Fact]
    public void Constructor_parameters_should_set_the_underlying_condition_properties()
    {
        Expression<Func<Customer, bool>> expression = c => c.CustomerNo ==1;
        
        var theCondition = new CustomCondition<Customer>("Custom", c => c.CustomerNo == 1, "Customer no should be 1", "CustomEvaluator",
                                                        EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll));  
        
        theCondition.Should().Match<Condition<Customer>>(c => c.AdditionalInfo.Count == 0 && c.ContextType == typeof(Customer) && c.EventDetails != null
                                                          && c.CompiledPredicate != null && c.ConditionName == "Custom" && c.EvaluatorTypeName == "CustomEvaluator"
                                                          && c.ToEvaluate == expression.ToString()
                                                          && c.IsLambdaPredicate == true);
    }

    [Fact]
    public void Constructor_parameters_should_set_the_underlying_condition_properties_without_a_lambda_predicate()
    {
        var theCondition = new CustomCondition<Customer>("Custom","Some text to evaluate", "Failure message", "CustomEvaluator",
                                                        EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll));

        theCondition.Should().Match<Condition<Customer>>(c => c.AdditionalInfo.Count == 0 && c.ContextType == typeof(Customer) && c.EventDetails != null
                                                          && c.CompiledPredicate == null && c.ConditionName == "Custom" && c.EvaluatorTypeName == "CustomEvaluator"
                                                          && c.ToEvaluate == "Some text to evaluate"
                                                          && c.IsLambdaPredicate == false);
    }
}
