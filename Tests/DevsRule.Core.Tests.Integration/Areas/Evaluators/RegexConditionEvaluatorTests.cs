using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Utilities;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace DevsRule.Core.Tests.Integration.Areas.Evaluators;

public class RegexConditionEvaluatorTests : IClassFixture<ConditionEngineFixture>
{

    private readonly ConditionEngine _conditionEngine;
    public RegexConditionEvaluatorTests(ConditionEngineFixture conditionEngineFixture)
    
        => _conditionEngine = conditionEngineFixture.ConditionEngine;

    
    [Fact]
    public async Task Should_pass_if_the_IgnoreCase_flag__was_set()
    {
        var supplier = new Supplier("SUPPLIER", 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Supplier>("RegexOne", s => s.SupplierName, "^supplier", "Should be upper case", RegexOptions.IgnoreCase)
                    .WithFailureValue("10")
                    .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(supplier).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeTrue();
    }
    [Fact]
    public async Task Should_fail_if_the_IgnoreCase_flag_was_not_set()
    {
        var supplier = new Supplier("SUPPLIER", 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Supplier>("RegexOne", s => s.SupplierName, "^supplier", "Should be upper case")
                    .WithFailureValue("10")
                    .CreateRule();


        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(supplier).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeFalse();
    }



    [Fact]
    public async Task Should_fail_if_the_culure_invariant_flag_was_not_set()
    {
        var customer = new Customer("PhIl", 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^Phil", "Should be Phil", RegexOptions.IgnoreCase)
                    .WithFailureValue("10")
                    .CreateRule();

        /*
            * Ignore case will work with our cutlure but fail for Turkish because there is no equivalent of a lower case I, so CultureInvariant needs to be set. 
            * Could not think of an example where I did not need to use both the ignore case flag in conjuction with the culture flag, so this will have to do for now.
         */ 
        Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeFalse();

    }
    [Fact]
    public async Task Should_pass_if_the_culure_invariant_flag_was__set()
    {
        var customer = new Customer("PhIl", 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^Phil", "Should be Phil", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                    .WithFailureValue("10")
                    .CreateRule();

        /*
            * Ignore case will work with our cutlure but fail for Turkish because there is no equivalent of a lower case I, so CultureInvariant needs to be set. 
            * Could not think of an example where I did not need to use both the ignore case flag in conjuction with the culture flag, so this will have to do for now.
         */
        Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeTrue();

    }

    [Fact]
    public async Task Should_fail_if_the_multiline_flag_is_was_not_set()
    {
        /*
            * Customer name is two lines ^ $ start and end of input line is affected by Multiline
            * It should fail as from the start to the end does not just match CustomerOne, the engine will see ^CustomerOneCustomerTwo$
         */

        var customer = new Customer("CustomerTwo\nCustomerOne", 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^CustomerOne$", "Should be CustomerOne")
                    .WithFailureValue("10")
                    .CreateRule();


        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeFalse();

    }
    [Fact]
    public async Task Should_pass_if_the_multiline_flag_was_set()
    {
        /*
            * Customer name is two lines ^ $ start and end of input line is affected by Multiline
            * It will pass with Muliline because there is a CustomerOne which matches the start and end of the line
         */ 
        
        var customer = new Customer("CustomerTwo\nCustomerOne", 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^CustomerOne$", "Should be CustomerOne", RegexOptions.Multiline)
                    .WithFailureValue("10")
                    .CreateRule();


        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeTrue();

    }

    [Fact]
    public async Task Should_fail_if_the_singleline_flag_was_not_set()
    {
        /*
            * The pattern uses the . character which matches every character except \n unless the single line option is set. The + is just tt repeat it (one or more times)
            * This test has \n but its ignored so the test passes
         */
        var customerName = @"Customer
                             One";

        var customer = new Customer(customerName, 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, @"^.+$", "Should be pretty much anything")
                    .WithFailureValue("10")
                    .CreateRule();


        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeFalse();

    }
    [Fact]
    public async Task Should_pass_if_the_singleline_flag_was_set()
    {
        /*
            * The pattern uses the . character which matches every character except \n unless the single line option is set. The * is just tt repeat it (one or more times)
         */
        var customerName = @"Customer
                             One";

        var customer = new Customer(customerName, 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, @"^Cus.*$", "Should start with Cus", RegexOptions.Singleline)
                    .WithFailureValue("10")
                    .CreateRule();


        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeTrue();

    }


    [Theory]
    [InlineData("Customer 1")]
    [InlineData("Cust One")]
    [InlineData("Cust123")]
    public async Task Should_pass_any_characters_excluding_new_line_given_at_least_one_and_at_most_10(string customerName)
    {
        var customer = new Customer(customerName, 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^.{1,10}$","Should be upper case")
                    .WithFailureValue("10")
                    .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeTrue();

    }

    [Fact]
    public async Task Should_fail_when_more_than_10_characters_excluding_new_line()
    {
        var customer = new Customer("Customer Name", 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^.{1,10}$", "Should be between 1 and 10 characters")
                     .WithoutFailureValue()
                    .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeFalse();

    }

    [Fact]
    public async Task Should_fail_if_the_IgnorePatternWhitespace_flag_was_not_set()
    {
        var supplier = new Supplier("Supplier", 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Supplier>("RegexOne", s => s.SupplierName, "^Supplier  $", "Should not ignore whitespace in pattern")
                    .WithFailureValue("10")
                    .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(supplier).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Should_pass_if_the_IgnorePatternWhitespace_flag_was_set()
    {
        var supplier = new Supplier("Supplier", 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "5")
                            .WithRegexCondition<Supplier>("RegexOne", s => s.SupplierName, "^Supplier  $", "Should not ignore whitespace in pattern",RegexOptions.IgnorePatternWhitespace)
                    .WithFailureValue("10")
                    .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(supplier).Create(), _conditionEngine.EventPublisher);

        theResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task The_evaluation_should_indicate_failure_if_an_exception_occurs()
    {
        //malformed pattern to cause an exception

        var additionalInfo = GeneralUtils.CreateDictionaryForRegex("^([A-", RegexOptions.None);

        var conditionSet = new ConditionSet("SetOne", new RegexCondition<Customer>("RegexCondition",c => c.CustomerName, "Should be CustomerOne",additionalInfo));

        var theConditionResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName,RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher,CancellationToken.None);

        //can be any exception just using the parse as that is what occurs in this instance
        theConditionResult.Should().Match<ConditionResult>(e => e.IsSuccess == false && e.Exception!.GetType() == typeof(RegexParseException));

    }

    [Fact]
    public async Task Should_fail_without_an_exception_if_the_property_value_is_null()
    {
        var customer = new Customer(null!, 1, 1, 1);

        var rule = RuleBuilder
                    .WithName("RuleOne")
                        .ForConditionSetNamed("SetOne")
                            .WithRegexCondition<Customer>("RegexOne", c => c.CustomerName, "^[A-Z]{1,10}$", "Should be upper case")
                    .WithoutFailureValue()
                    .CreateRule();

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions.Count == 0);

    }

    [Fact]
    public async Task A_key_with_an_item_value_of_null_should_not_cause_any_exceptions()
    {

        var additionalInfo = GeneralUtils.CreateDictionaryForRegex("^[A-Z]{1,10}$", RegexOptions.None, ("NoValueForKey", null!));

        var customer = new Customer("CUSTOMER", 1, 1, 1);

        var regexCondition = new RegexCondition<Customer>("RegexOne", c => c.CustomerName, "Should be upper case", additionalInfo);
        var conditionSet = new ConditionSet("SetOne", regexCondition);
        var rule = new Rule("RuleOne", conditionSet);

        var theResult = await rule.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(customer).Create(), _conditionEngine.EventPublisher);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == true && r.Exceptions.Count == 0);

    }

    [Fact]
    public async Task Incorrect_combinations_of_regex_options_should_cause_a_failure_instead_of_an_exception_as_of_msdn_re_regex_options()
    {
        //The RegexOptions.ECMAScript option can be combined only with the RegexOptions.IgnoreCase and RegexOptions.Multiline options. The use of any other option in a regular expression results in an ArgumentOutOfRangeException.

        var additionalInfo = GeneralUtils.CreateDictionaryForRegex("[A-Za-z]+$", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.ECMAScript | RegexOptions.IgnoreCase);

        var customer = new Customer("CUSTOMER", 1, 1, 1);

        var regexCondition  = new RegexCondition<Customer>("RegexOne", c => c.CustomerName, "Should be upper case", additionalInfo);
        var conditionSet    = new ConditionSet("SetOne", regexCondition);
        var rule            = new Rule("RuleOne", conditionSet);
        var contexts        = RuleDataBuilder.AddForAny(customer).Create();

        (await rule.Evaluate(_conditionEngine.GetEvaluatorByName,contexts, _conditionEngine.EventPublisher)).Should().Match<RuleResult>(r => r.IsSuccess == false);
    }
    [Fact]
    public async Task Correct_combinations_of_regex_options_such_as_those_combined_with_ecmascritp_should_be_pass()
    {
        //The RegexOptions.ECMAScript option can be combined only with the RegexOptions.IgnoreCase and RegexOptions.Multiline options. The use of any other option in a regular expression results in an ArgumentOutOfRangeException.

        var additionalInfo = GeneralUtils.CreateDictionaryForRegex("[A-Za-z]+$", RegexOptions.Multiline | RegexOptions.ECMAScript | RegexOptions.IgnoreCase);

        var customer = new Customer("CUSTOMER", 1, 1, 1);

        var regexCondition = new RegexCondition<Customer>("RegexOne", c => c.CustomerName, "Should be upper case", additionalInfo);
        var conditionSet = new ConditionSet("SetOne", regexCondition);
        var rule = new Rule("RuleOne", conditionSet);

        var contexts = RuleDataBuilder.AddForAny(customer).Create();

        (await rule.Evaluate(_conditionEngine.GetEvaluatorByName,contexts, _conditionEngine.EventPublisher)).Should().Match<RuleResult>(r => r.IsSuccess == true);

    }
}
