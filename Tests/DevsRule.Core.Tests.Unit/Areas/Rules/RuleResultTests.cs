using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class RuleResultTests
{


    [Fact]
    public void Should_set_all_properties_from_constructor_arguments_in_the_rule_results_class()
    {
        var theConditionResult = new ConditionResult("SetName", "SetValue", "ConditionName", 0, "ContextType", "ToEValuate", 42, "EvaluatedBy", true, "FailureMessage", 1, 1, "DataTenantID", null);
        var theRuleResult = new RuleResult("RuleName", "FailureValue", theConditionResult,"RuleTenantID", new List<string>(), new List<Exception>(), 1, 1000, true);

        theRuleResult.Should().Match<RuleResult>(r => r.EvaluationChain != null && r.Exceptions.Count == 0 && r.FailureMessages.Count == 0
                                                     && r.FailureValue == "FailureValue" && r.IsSuccess == true && r.RuleDisabled == true
                                                     && r.RuleName == "RuleName" && r.RuleResultChain == null && r.RuleTimeMicroseconds == 1000 && r.RuleTimeMilliseconds == 1
                                                     && r.SuccessfulSet == "SetName" && r.SuccessValue == "SetValue" && r.TotalEvaluations == 1
                                                     && r.DataTenantID == "DataTenantID" && r.RuleTenantID == "RuleTenantID");



    }
}
