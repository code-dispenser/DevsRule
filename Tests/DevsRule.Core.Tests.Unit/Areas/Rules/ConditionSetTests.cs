using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;


public class ConditionSetTests : IClassFixture<ConditionEngineFixture>
{

    private readonly ConditionEngine _conditionEngine;

    public ConditionSetTests(ConditionEngineFixture conditionEngineFixture)

        => _conditionEngine = conditionEngineFixture.ConditionEngine;
    


    [Fact]
    public async Task One_condition_in_a_condition_set_should_have_a_null_evalutaion_chain()
    {

        var theResult = await new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message))
                 .EvaluateConditions(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher, CancellationToken.None);
        
        theResult.EvaluationtChain.Should().BeNull();
    }

    [Fact]
    public async Task Two_conditions_with_the_first_passing_and_the_other_failiing_should_return_a_fail_with_the_passing_condition_in_the_evaluation_chain()
    {
   
        var setResult = await new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message))
                            .AndCondition(new PredicateCondition<Customer>("Member Years", c => c.MemberYears == 5, StaticData.Customer_Three_Member_Years))
                                .EvaluateConditions(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher, CancellationToken.None);

        using (new AssertionScope())
        {
            setResult.EvaluationtChain.Should().NotBeNull();
            setResult.EvaluationtChain?.IsSuccess.Should().BeTrue();
        }

    }
    [Fact]
    public async Task Two_conditions_with_the_first_failing_should_return_a_fail_with_no_evaluation_chain()
    {
        var thetResult = await new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name",c => c.CustomerName == "CustomerTwo", StaticData.Customer_One_Name_Message))
                                    .AndCondition(new PredicateCondition<Customer>("Member Years",c => c.MemberYears == 5, StaticData.Customer_One_Member_Years))
                                        .EvaluateConditions(_conditionEngine.GetEvaluatorByName,RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher, CancellationToken.None);

        thetResult.Should().NotBeNull()
                        .And.Match<ConditionResult>(r => r.IsSuccess == false && r.EvaluationtChain == null);

    }

    [Fact]
    public async Task Three_conditions_with_the_first_two_passing_with_the_last_failing_should_return_a_fail_with_the_two_passes_in_the_evaluation_chain()
    {
        var thetResult = await new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message))
                            .AndCondition(new PredicateCondition<Customer>("Member years", c => c.MemberYears == 1, StaticData.Customer_One_Member_Years))
                                .AndCondition(new PredicateCondition<Customer>("Total Spend", c => c.TotalSpend == 5M, StaticData.Customer_One_Spend_Message))
                                    .EvaluateConditions(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher, CancellationToken.None);


        List<ConditionResult> conditionResults = new();

        var theResultChain = thetResult;

        while (theResultChain != null)
        {
            conditionResults.Add(theResultChain);

            theResultChain = theResultChain.EvaluationtChain;
        }
        using (new AssertionScope())
        {

            conditionResults[0].Should().Match<ConditionResult>(r => r.IsSuccess == false && r.ConditionSetIndex == 2 && r.EvaluationtChain != null);
            conditionResults[1].Should().Match<ConditionResult>(r => r.IsSuccess == true && r.ConditionSetIndex == 1 && r.EvaluationtChain != null);
            conditionResults[2].Should().Match<ConditionResult>(r => r.IsSuccess == true && r.ConditionSetIndex == 0 && r.EvaluationtChain == null);
        }

    }

    [Fact]
    public async Task A_condition_to_be_evaluated_without_an_evaluator_should_fail_with_the_exceptions_added_to_the_condition_result()

        => (await new ConditionSet("Set One", new Condition<Customer>("Customer Name", "to evaluate string", "failure message", "NoEvaluator", false))
                        .EvaluateConditions(_conditionEngine.GetEvaluatorByName,RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher, CancellationToken.None))
                            .Should().Match<ConditionResult>(r => r.IsSuccess == false && r.Exception!.GetType() == typeof(MissingConditionEvaluatorException));

    [Fact]
    public void Should_be_able_to_remove_a_condition_by_from_a_set()
    {
        var setOne = new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message))
                            .AndCondition(new PredicateCondition<Customer>("Member years", c => c.MemberYears == 1, StaticData.Customer_One_Member_Years))
                            .AndCondition(new PredicateCondition<Customer>("Total Spend", c => c.TotalSpend == 5M, StaticData.Customer_One_Spend_Message));


        var conditionCount  = setOne.Conditions.Count;
        var removedName     = setOne.Conditions[1].ConditionName;
        var condition       = setOne.Conditions[1];

        setOne.RemoveCondition(condition);
        using (new AssertionScope())
        {
            setOne.Conditions.Count.Should().Be(conditionCount - 1);
            setOne.Conditions.Any(c => c.ConditionName == removedName).Should().BeFalse();
        }
    }

    [Fact]
    public void Should_be_able_to_remove_a_condition_by_from_a_set_by_its_index()
    {
        var setOne = new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message))
                            .AndCondition(new PredicateCondition<Customer>("Member years", c => c.MemberYears == 1, StaticData.Customer_One_Member_Years))
                            .AndCondition(new PredicateCondition<Customer>("Total Spend", c => c.TotalSpend == 5M, StaticData.Customer_One_Spend_Message));

        var conditionCount  = setOne.Conditions.Count;
        var removedName     = setOne.Conditions[1].ConditionName;

        setOne.RemoveConditionByIndex(1);

        using(new AssertionScope())
        {
            setOne.Conditions.Count.Should().Be(conditionCount - 1);
            setOne.Conditions.Any(c => c.ConditionName == removedName).Should().BeFalse();
        }

        
    }
    [Fact]
    public void Passing_an_incorrect_index_to_remove_a_condition_should_not_throw_an_exception()
    {
        var setOne = new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne", StaticData.Customer_One_Name_Message));

        setOne.RemoveConditionByIndex(100);

        using (new AssertionScope())
        {
            setOne.Conditions.Count.Should().Be(1);
        }

    }

    [Fact]
    public async Task Should_be_able_to_replace_failure_message_place_holders_with_property_values_if_the_condition_fails()
    {
        Customer customer = new Customer("Major Corp.", 999, 999_999, 5, new Address("Major Corp Street", "Camden", "London", "NW1 LLL"));

        var setOne = new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne" && c.TotalSpend >= 1_000_000,
                                                         "@{CustomerName}, @{Address.Town}, @{Address.PostCode}, has only spent @{TotalSpend} which does not meet the requirment of a total spend of 1,000,000 or more"));

        var theConditionResult = await setOne.EvaluateConditions(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher, CancellationToken.None);


        theConditionResult.FailureMessage.Should().Be("Major Corp., Camden, NW1 LLL, has only spent 999999 which does not meet the requirment of a total spend of 1,000,000 or more");

    }

    [Fact]
    public async Task A_condition_set_recieving_contexts_that_have_a_null_data_field_should_throw_a_missing_rule_contexts_exception()
    {
        var setOne = new ConditionSet("Set One", new PredicateCondition<Customer>("Customer Name", c => c.CustomerName == "CustomerOne" && c.TotalSpend >= 1_000_000, "Total spend should be greater than or equal to 1,000,000"));

        await FluentActions.Invoking(() => setOne.EvaluateConditions(_conditionEngine.GetEvaluatorByName, new RuleData(null!), _conditionEngine.EventPublisher, CancellationToken.None))
            .Should().ThrowExactlyAsync<MissingRuleContextsException>();

        

    }

    [Fact]
    public void Should_throw_an_argument_exception_when_adding_a_condition_that_is_not_assignable_from_iCondition_tContext()
    {
        FluentActions.Invoking(() => new ConditionSet("SetOne", new FakeCondition(typeof(string)))).Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Should_throw_a_missing_rule_contexts_exception_if_rule_data_null_or_contexts_are_null()
    {
        var theCoditionSet = new ConditionSet("SetOne", new PredicateCondition<Customer>("CustomerName", c => c.CustomerName == "CustomerOne", "Should be CustomerOne"));
        
        using(new AssertionScope())
        {
            FluentActions.Invoking(() => theCoditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, null!, _conditionEngine.EventPublisher, CancellationToken.None))
                .Should().ThrowExactlyAsync<MissingRuleContextsException>();

            FluentActions.Invoking(() => theCoditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForCondition("test", null!).Create(), _conditionEngine.EventPublisher, CancellationToken.None))
                .Should().ThrowExactlyAsync<MissingRuleContextsException>();

            FluentActions.Invoking(() => theCoditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, new RuleData(null!), _conditionEngine.EventPublisher, CancellationToken.None))
                .Should().ThrowExactlyAsync<MissingRuleContextsException>();

        }
    }
}


