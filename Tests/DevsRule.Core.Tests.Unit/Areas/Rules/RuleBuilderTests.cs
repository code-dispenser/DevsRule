using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.RegularExpressions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class RuleBuilderTests
{
    [Fact]
    public void The_rule_should_contain_three_sets_with_a_total_of_five_conditions_in_the_order_they_were_added()
    {
        //AssertionOptions.FormattingOptions.MaxDepth = 1000;

        var theRule = RuleBuilder
            .WithName("RuleOne")
                .ForConditionSetNamed("SetOne")
                    .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne")
                    .AndPredicateCondition<Supplier>("SupName", s => s.SupplierName == "Supplier name", "should be SupplierOne")
                    .AndCustomPredicateCondition<Customer>("CustYears", c => c.MemberYears > 1, "Member years should be greater than 1", "CustomPredicateConditionEvaluator")
                .OrConditionSetNamed("SetTwo")
                    .WithCustomPredicateCondition<Customer>("CustNo", c => c.CustomerNo == 111, "Customer No. should be 111",  "CustomPredicateConditionEvaluator")
                    .AndRegexCondition<Customer>("CustAddressLine", c => c.Address!.AddressLine, "^CustomerOne$", "AddressLine should be something", RegexOptions.None)
                .OrConditionSetNamed("SetThree")
                    .WithCustomCondition<Supplier>("SupCustom", "some value", "Should be some value", "CustomEvaluatorName", new Dictionary<string, string>())
                    .AndCustomCondition<Supplier>("SupCustom", "some value", "Should be some value", "CustomEvaluatorName",null!) //should not be added as duplicate name
                    .AndCustomCondition<Customer>("CustTwoYears", "2", "Customer years should be 2", "CustomEvaluatorName", new Dictionary<string, string>() { ["One"]="1" })
                    .AndCustomCondition<Customer>("CustTwoYears", "2", "Customer years should be 2", "CustomEvaluatorName", new Dictionary<string, string>() { ["One"]="1" })//should not be added as duplicate name
                .WithFailureValue("10")
            .CreateRule();

        var theConditionSetOne      = theRule.ConditionSets[0];
        var theConditionSetTwo      = theRule.ConditionSets[1];
        var theConditionSetThree    = theRule.ConditionSets[2];

        using (new AssertionScope())
        {
            theRule.Should().Match<Rule>(r => r.RuleName == "RuleOne" && r.FailureValue == "10" && r.ConditionSets.Count == 3);

            theConditionSetOne.Should().Match<ConditionSet>(c => c.SetValue == String.Empty && c.ConditionSetName == "SetOne" && c.Conditions.Count == 3);

            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[0]).ConditionName.Should().Be("CustName");
            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[0]).FailureMessage.Should().Be("Customer name should CustomerOne");
            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[0]).ToEvaluate.Should().Be("c => (c.CustomerName == \"CustomerOne\")");
            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[0]).EvaluatorTypeName.Should().Be("PredicateConditionEvaluator");

            ((PredicateCondition<Supplier>)theConditionSetOne.Conditions[1]).ConditionName.Should().Be("SupName");
            ((PredicateCondition<Supplier>)theConditionSetOne.Conditions[1]).FailureMessage.Should().Be("should be SupplierOne");
            ((PredicateCondition<Supplier>)theConditionSetOne.Conditions[1]).ToEvaluate.Should().Be("s => (s.SupplierName == \"Supplier name\")");

            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[2]).ConditionName.Should().Be("CustYears");
            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[2]).FailureMessage.Should().Be("Member years should be greater than 1");
            ((PredicateCondition<Customer>)theConditionSetOne.Conditions[2]).ToEvaluate.Should().Be("c => (c.MemberYears > 1)");

            theConditionSetTwo.Should().Match<ConditionSet>(c => c.SetValue == String.Empty && c.ConditionSetName == "SetTwo" && c.Conditions.Count == 2);

            ((PredicateCondition<Customer>)theConditionSetTwo.Conditions[0]).ConditionName.Should().Be("CustNo");
            ((PredicateCondition<Customer>)theConditionSetTwo.Conditions[0]).FailureMessage.Should().Be("Customer No. should be 111");
            ((PredicateCondition<Customer>)theConditionSetTwo.Conditions[0]).ToEvaluate.Should().Be("c => (c.CustomerNo == 111)");

            ((RegexCondition<Customer>)theConditionSetTwo.Conditions[1]).ConditionName.Should().Be("CustAddressLine");
            ((RegexCondition<Customer>)theConditionSetTwo.Conditions[1]).FailureMessage.Should().Be("AddressLine should be something");
            ((RegexCondition<Customer>)theConditionSetTwo.Conditions[1]).ToEvaluate.Should().Be("Address.AddressLine");
            ((RegexCondition<Customer>)theConditionSetTwo.Conditions[1]).AdditionalInfo["Pattern"].Should().Be("^CustomerOne$");
            ((RegexCondition<Customer>)theConditionSetTwo.Conditions[1]).EvaluatorTypeName.Should().Be("RegexConditionEvaluator");

            theConditionSetThree.Should().Match<ConditionSet>(c => c.SetValue == String.Empty && c.ConditionSetName == "SetThree" && c.Conditions.Count == 2);

            ((Condition<Supplier>)theConditionSetThree.Conditions[0]).ConditionName.Should().Be("SupCustom");
            ((Condition<Supplier>)theConditionSetThree.Conditions[0]).FailureMessage.Should().Be("Should be some value");
            ((Condition<Supplier>)theConditionSetThree.Conditions[0]).ToEvaluate.Should().Be("some value");

            ((Condition<Customer>)theConditionSetThree.Conditions[1]).ConditionName.Should().Be("CustTwoYears");
            ((Condition<Customer>)theConditionSetThree.Conditions[1]).FailureMessage.Should().Be("Customer years should be 2");
            ((Condition<Customer>)theConditionSetThree.Conditions[1]).ToEvaluate.Should().Be("2");
            ((Condition<Customer>)theConditionSetThree.Conditions[1]).EvaluatorTypeName.Should().Be("CustomEvaluatorName");

        }
    }

    [Fact]
    public void The_rule_builder_should_add_rule_and_condition_events_correctly()
    {
        var theRule = RuleBuilder
                        .WithName("RuleOne", EventDetails.Create<RuleResultEvent>(EventWhenType.OnFailure, PublishMethod.FireAndForget))
                            .ForConditionSetNamed("SetOne")
                                .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Customer name should CustomerOne",
                                    EventDetails.Create<ConditionResultEvent>(EventWhenType.Never, PublishMethod.WaitForAll))

                                .AndPredicateCondition<Supplier>("SupName", s => s.SupplierName == "Supplier name", "should be SupplierOne",
                                    EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccess, PublishMethod.WaitForAll))

                                .AndCustomPredicateCondition<Customer>("CustYears", c => c.MemberYears > 1, "Member years should be greater than 1", "CustomPredicateConditionEvaluator", null,
                                    EventDetails.Create<ConditionResultEvent>(EventWhenType.OnFailure, PublishMethod.FireAndForget))

                                .AndRegexCondition<Customer>("CustAddressLine", c => c.Address!.AddressLine, "^CustomerOne$", "AddressLine should be something", RegexOptions.None,
                                    EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.FireAndForget))

                           .WithoutFailureValue()
                        .CreateRule();


        var theRuleEvent                = theRule.RuleEventDetails;
        var custNameConditionEvent      = theRule.ConditionSets[0].Conditions[0].EventDetails;
        var supNameConditionEvent       = theRule.ConditionSets[0].Conditions[1].EventDetails;
        var custYearsConditionevent     = theRule.ConditionSets[0].Conditions[2].EventDetails;
        var custAddressConditionEvent   = theRule.ConditionSets[0].Conditions[3].EventDetails;


        using (new AssertionScope())
        {
            theRuleEvent.Should().Match<EventDetails>(e => e.PublishMethod == PublishMethod.FireAndForget && e.EventWhenType == EventWhenType.OnFailure);

            custNameConditionEvent.Should().Match<EventDetails>(e => e.EventWhenType == EventWhenType.Never && e.PublishMethod == PublishMethod.WaitForAll);

            supNameConditionEvent.Should().Match<EventDetails>(e => e.EventWhenType == EventWhenType.OnSuccess && e.PublishMethod == PublishMethod.WaitForAll);

            custYearsConditionevent.Should().Match<EventDetails>(e => e.EventWhenType == EventWhenType.OnFailure && e.PublishMethod == PublishMethod.FireAndForget);

            custAddressConditionEvent.Should().Match<EventDetails>(e => e.EventWhenType == EventWhenType.OnSuccessOrFailure && e.PublishMethod == PublishMethod.FireAndForget);
        }


    }

}
