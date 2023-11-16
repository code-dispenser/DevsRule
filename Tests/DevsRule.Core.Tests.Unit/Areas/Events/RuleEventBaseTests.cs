using DevsRule.Core.Areas.Events;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Events;

public  class RuleEventBaseTests
{
    [Fact]
    public void The_rule_event_base_class_properties_should_be_accessible_via_the_derived_class()
    {
        var ruleResultEvent = new RuleResultEvent("SomeSender", true, "SomeValue", "SomeFailureValue", "TenantID", new());


        ruleResultEvent.Should().BeAssignableTo<IEvent>().And.BeAssignableTo<RuleEventBase>()
                            .And.Match<RuleResultEvent>(r => r.IsSuccessEvent == true && r.SenderName == "SomeSender" && r.TenantID == "TenantID" && r.ExecutionExceptions.GetType() == typeof(List<Exception>)
                                                        && r.SuccessValue == "SomeValue" && r.FailureValue == "SomeFailureValue" && r.ExecutionExceptions.Count == 0);

    }

    [Fact]
    public void A_null_execution_list_passed_to_a_rule_event_base_should_be_converted_to_an_empty_list_by_the_constructor()
    {
        var theRuleResultEvent = new RuleResultEvent("SomeSender", true, "SomeValue", "SomeFailureValue", "TenantID", null!);

        theRuleResultEvent.ExecutionExceptions.Should().HaveCount(0);

    }
}
