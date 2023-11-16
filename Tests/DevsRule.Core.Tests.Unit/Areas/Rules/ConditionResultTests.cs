using DevsRule.Core.Common.Models;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class ConditionResultTests
{
    [Fact]
    public void The_condition_result_properties_should_be_populated_correctly()
    {
        var theConditionResult = new ConditionResult("SetName", "SomeStringValue", "ConditionName", 0, "Customer", "c => c.Name == Test", StaticData.CustomerOne(), "TheEvaluator", true, "Works on my machine", 100, 1000, "TenantID", new Exception());


        theConditionResult.Should().Match<ConditionResult>(c => c.ConditionName == "ConditionName"
                                                             && c.ConditionSetIndex == 0
                                                             && c.ContextType == "Customer"
                                                             && c.EvalMicroseconds == 100
                                                             && c.EvaluatedBy == "TheEvaluator"
                                                             && c.EvaluationData!.Equals(StaticData.CustomerOne())
                                                             && c.EvaluationtChain == null
                                                             && c.Exception!.GetType() == typeof(Exception)
                                                             && c.FailureMessage == "Works on my machine"
                                                             && c.IsSuccess == true
                                                             && c.SetName == "SetName"
                                                             && c.SetValue == "SomeStringValue"
                                                             && c.ToEvaluate == "c => c.Name == Test"
                                                             && c.TotalMicroseconds == 1000
                                                             && c.TenantID == "TenantID");



    }

    [Fact]
    public void Should_assign_empty_strings_to_nulls_from_the_constructor_arguements_for_various_properties_of_the_condition_result_class()
    {
        var theConditionResult = new ConditionResult(null!, null!, null!, 0, null!, null!, null, null!, false, null!, 0, 0, null!);

        theConditionResult.Should().Match<ConditionResult>(r => r.SetName == string.Empty && r.SetValue == string.Empty && r.ContextType == string.Empty
                                                          && r.ToEvaluate == string.Empty && r.FailureMessage == string.Empty && r.ConditionName == string.Empty
                                                          && r.EvaluatedBy == string.Empty);
    }
}
