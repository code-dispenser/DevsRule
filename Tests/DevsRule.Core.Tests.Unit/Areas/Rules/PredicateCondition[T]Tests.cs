using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Linq.Expressions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;


public class PredicateConditionTests
{
    [Fact]
    public void Should_throw_argument_null_exception_if_condition_is_null()

        => FluentActions.Invoking(() => new PredicateCondition<Customer>("Some condition", null!, "Failed"))
                            .Should().ThrowExactly<ArgumentNullException>();

    [Fact()]
    public void The_type_name_should_be_populated_given_a_context_and_a_condition()

        => new PredicateCondition<Customer>("Customer One", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message)
                        .ContextType.FullName.Should().Be("DevsRule.Tests.SharedDataAndFixtures.Models.Customer");

    [Fact]
    public void The_condition_string_should_be_populated_with_a_string_representation_of_the_condition()
    {
        Expression<Func<Customer, bool>> condition = (c) => c.CustomerName == "CustomerOne";

        new PredicateCondition<Customer>("Customer One", condition, StaticData.Customer_One_Name_Message)
                .ToEvaluate
                    .Should().Be(condition.ToString());
    }

    [Fact]
    public void The_condition_should_have_a_compiled_predicate()
    {
        var condition = new PredicateCondition<Customer>("Customer One", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message);

        var compiledFunc = condition.CompiledPredicate!;

        var thePredicateResult = compiledFunc(StaticData.CustomerOne());

        thePredicateResult.Should().BeTrue();

    }

    [Fact]
    public void The_predicate_constructor_chaining_should_pass_the_correct_data()
    {
        var theCondition = new PredicateCondition<Customer>("Customer One", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message, 
                                                         EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccess,PublishMethod.FireAndForget));

        using(new AssertionScope())
        {
            theCondition.Should().Match<PredicateCondition<Customer>>(c => c.EventDetails != null && c.EvaluatorTypeName == GlobalStrings.Predicate_Condition_Evaluator && c.AdditionalInfo != null
                                                                  && c.IsLambdaPredicate == true && c.CompiledPredicate != null && c.ConditionName == "Customer One"
                                                                  && c.ContextType == typeof(Customer)  && c.FailureMessage == StaticData.Customer_One_Name_Message);
        }
       

    }


}
