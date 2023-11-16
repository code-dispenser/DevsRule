using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;



namespace DevsRule.Core.Tests.Unit.Common.Models;


public class SimpleTypeTests
{
    [Fact]
    public void The_cache_key_should_return_the_values_it_was_assigned()
    {
        var theCacheKey = new CacheKey("TheItemName");

        var keepTestCoverageHappyCacheKey = theCacheKey with { ItemName = "NewItemName", TenantID = "TenantOne", CultureID = "en-US" };

        using (new AssertionScope())
        {
            theCacheKey.Should().Match<CacheKey>(c => c.ItemName == "TheItemName" && c.TenantID == GlobalStrings.Default_TenantID && c.CultureID == GlobalStrings.Default_CultureID);

            keepTestCoverageHappyCacheKey.Should().Match<CacheKey>(c => c.ItemName == "NewItemName" && c.TenantID == "TenantOne" && c.CultureID == "en-US");
        }

    }
    [Fact]
    public void The_cache_item_should_return_the_value_it_was_assigned()
    {
        var theCacheItem = new CacheItem(StaticData.CustomerOne());

        var keepTestCoverageHappyCacheItem = theCacheItem with { Value = StaticData.CustomerTwo() };

        using (new AssertionScope())
        {
            theCacheItem.Value.Should().BeOfType<Customer>().And.Match<Customer>(c => c.CustomerName == "CustomerOne");

            keepTestCoverageHappyCacheItem.Value.Should().BeOfType<Customer>().And.Match<Customer>(c => c.CustomerName == "CustomerTwo");
        }

    }

    [Fact]
    public void An_evaluation_result_should_show_a_success_or_failure_and_hold_the_exception_if_one_occurred()
    {
        var theEvaluationResult = new EvaluationResult(false,"Failed message", new Exception());

        var keepCodeCoverageHappyResult = theEvaluationResult with { IsSuccess = true, Exception = null };

        using (new AssertionScope())
        {
            theEvaluationResult.Should().Match<EvaluationResult>(e => e.IsSuccess == false && e.Exception!.GetType() == typeof(Exception));
            keepCodeCoverageHappyResult.Should().Match<EvaluationResult>(e => e.IsSuccess == true && e.Exception == null);
        }


    }

    [Fact]
    public void Should_be_able_to_set_auto_properties_of_the_context_record_class_via_constructor_and_with()
    {
        var theDataContext  = new DataContext(42, "Condition Name");
        var newContext      = theDataContext with { ConditionName = "Changed", Data = 43 };
        var data            = (int)newContext.Data;//dynamic cant be used in the expression syntax for fluent assertions

        using (new AssertionScope())
        {
            newContext.ConditionName.Should().Be("Changed");
            data.Should().Be(43);
        }
    }

    [Fact]
    public void Should_be_able_to_set_auto_properties_of_the_evaluation_result_record_class_via_constructor_and_with()
    {

        var theEvaluationResult = new EvaluationResult(false, "Failure Message", new Exception());

        var withTheEvaluationResult = theEvaluationResult with { IsSuccess = false, FailureMeassage = "Failure Message" };

        theEvaluationResult.Should().Match<EvaluationResult>(r => r.IsSuccess == false && r.FailureMeassage == "Failure Message" && r.Exception != null);
    }


}
