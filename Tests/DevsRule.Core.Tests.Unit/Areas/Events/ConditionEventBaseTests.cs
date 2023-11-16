using DevsRule.Core.Areas.Events;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Events;

public  class ConditionEventBaseTests
{
    [Fact]
    public void The_condition_event_base_class_properties_should_be_accessible_via_the_derived_class()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), new SystemException());


        conditionResultEvent.Should().BeAssignableTo<IEvent>().And.BeAssignableTo<ConditionEventBase>()
            .And.Match<ConditionResultEvent>(e => e.IsSuccessEvent == true && e.ContextType == typeof(Customer) && e.SenderName == "SomeSender" && e.TenantID == "TenantID"
                                           && e.SerializationException!.GetType() == typeof(SystemException)  && e.ExecutionExceptions.GetType() == typeof(List<Exception>) && e.ExecutionExceptions.Count == 0);

    }

    [Fact]
    public void Try_get_data_should_return_an_object_that_can_cast_to_the_correct_type_if_there_were_no_serialization_issues()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), null);

        conditionResultEvent.TryGetData(out var contextData);//TryGetData needs SerializationException == null

        (contextData as Customer).Should().Match<Customer>(c => c.CustomerName == "CustomerOne" && c.CustomerNo == 111 && c.MemberYears ==1 && c.TotalSpend == 111.11M);

    }

    [Fact]
    public void Try_get_data_should_fail_if_there_was_a_serialization_issue()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(), "TenantID", new(), new SystemException());

        conditionResultEvent.TryGetData(out var contextData);

        contextData.Should().BeNull();

    }
    [Fact]
    public void Try_get_data_should_fail_if_the_json_is_null()
    {
        var conditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), null!, "TenantID", new(), null);

        conditionResultEvent.TryGetData(out var contextData);

        contextData.Should().BeNull();

    }

    [Fact]
    public void A_null_execution_list_passed_to_a_condition_event_base_should_be_converted_to_an_empty_list_by_the_constructor()
    {
        var theConditionResultEvent = new ConditionResultEvent("SomeSender", true, typeof(Customer), null!, "TenantID", null!, null);

        theConditionResultEvent.ExecutionExceptions.Should().HaveCount(0);

    }
}
