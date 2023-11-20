using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using FluentAssertions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Events;

public class EventDetailsTests
{
    [Fact]
    public void Should_return_the_correct_property_values()
    {
        var theEventDetails = new EventDetails("EventTypeName", EventWhenType.OnSuccessOrFailure, PublishMethod.FireAndForget);

        theEventDetails.Should().Match<EventDetails>(e => e.EventTypeName == "EventTypeName" && e.EventWhenType == EventWhenType.OnSuccessOrFailure 
                                                    && e.PublishMethod == PublishMethod.FireAndForget);
    }

    [Fact]
    public void The_static_to_json_rule_should_create_a_json_rule_event_details_class_from_event_details()
    {
        var eventDetails = new EventDetails(typeof(RuleResultEvent).AssemblyQualifiedName!, EventWhenType.OnSuccess, PublishMethod.FireAndForget);

        var theJsonRuleEventDetails = EventDetails.ToJsonRule(eventDetails);

        theJsonRuleEventDetails.Should().Match<JsonRule.EventDetails>(e => e.EventTypeName == typeof(RuleResultEvent).FullName && e.EventWhenType == EventWhenType.OnSuccess.ToString()
                                            && e.PublishMethod == PublishMethod.FireAndForget.ToString());
    }

    [Fact]
    public void The_static_from_json_rule_should_create_an_event_details_class_from_the_json_rule_event_details()
    {
        var jsonRuleEventDetails = new JsonRule.EventDetails { EventTypeName = typeof(RuleResultEvent).FullName!, EventWhenType = "OnFailure", PublishMethod = "WaitForAll" };

        var theEventDetails = EventDetails.FromJsonRule(jsonRuleEventDetails);

        theEventDetails.Should().Match<EventDetails>(e => e.EventTypeName == typeof(RuleResultEvent).AssemblyQualifiedName && e.EventWhenType == EventWhenType.OnFailure
                                            && e.PublishMethod == PublishMethod.WaitForAll);
    }

    [Fact]
    public void The_static_from_json_rule_when_matching_the_event_type_name_should_add_dot_if_one_is_not_present_to_match_via_ends_with()
    {
        var jsonRuleEventDetails = new JsonRule.EventDetails { EventTypeName = nameof(RuleResultEvent), EventWhenType = "OnFailure", PublishMethod = "WaitForAll" };

        var theEventDetails = EventDetails.FromJsonRule(jsonRuleEventDetails);

        theEventDetails.Should().Match<EventDetails>(e => e.EventTypeName == typeof(RuleResultEvent).AssemblyQualifiedName && e.EventWhenType == EventWhenType.OnFailure
                                            && e.PublishMethod == PublishMethod.WaitForAll);
    }

    [Fact]
    public void Converting_bad_json_event_details_should_return_null()
    {
        var jsonEventDetails = new JsonRule.EventDetails { EventTypeName="NoDotInBadName", EventWhenType= null!, PublishMethod = null! };

        EventDetails theEventDetails = EventDetails.FromJsonRule(jsonEventDetails)!;

        theEventDetails.Should().BeNull();
    }

    [Fact]
    public void When_converting_json_event_details_a_dot_should_be_added()
    {
        var jsonEventDetails = new JsonRule.EventDetails { EventTypeName="NoDotInBadName", EventWhenType= null!, PublishMethod = null! };

        EventDetails theEventDetails = EventDetails.FromJsonRule(jsonEventDetails)!;

        theEventDetails.Should().BeNull();
    }
}
 