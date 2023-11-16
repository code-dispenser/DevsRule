using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Rules;

public class RegexConditionTTests
{
    [Fact]
    public void Should_throw_argument_null_exception_if_property_expression_is_null()
    {
        Dictionary<string, string> regexData = new Dictionary<string, string> { ["Pattern"]="^CustomerOne$" };

        FluentActions.Invoking(() => new RegexCondition<Customer>("Some Condition", null!, "Failed", regexData))
                        .Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Should_throw_argument_exception_if_regex_data_dictionary_does_not_contain_a_pattern_key()
    {
        Dictionary<string, string> regexData = new Dictionary<string, string> { ["attern"]="^CustomerOne$" };

        FluentActions.Invoking(() => new RegexCondition<Customer>("Some Condition", c => c.CustomerName,  StaticData.Customer_One_Name_Message, regexData))
                            .Should().ThrowExactly<MissingRegexPatternException>();
    }
    [Fact]
    public void The_type_name_should_be_populated_given_a_context_and_a_condition()
    {
        Dictionary<string, string> regexData = new Dictionary<string, string> { ["Pattern"]="^CustomerOne$" };

        new RegexCondition<Customer>("Some Condition", c => c.CustomerName, StaticData.Customer_One_Name_Message, regexData)
                        .ContextType.FullName.Should().Be("DevsRule.Tests.SharedDataAndFixtures.Models.Customer");
    }
    [Fact]
    public void The_condition_string_should_be_populated_with_a_string_representaion_of_the_condition()
    {
        Dictionary<string, string> regexData = new Dictionary<string, string> { ["Pattern"]="^CustomerOne$" };

        new RegexCondition<Customer>("Some Condition", c => c.CustomerName, StaticData.Customer_One_Name_Message,regexData)
                .ToEvaluate
                    .Should().Be($"CustomerName");
    }

    [Fact]
    public void Using_the_static_create_method_should_correctly_create_the_regex_condition()
    {
        //RegexOptions.None is the default so not written to the dictionary
        var regexCondition = RegexCondition<Customer>.Create("RegexCondition", c => c.MemberYears, "^[1-9]{1,2}$", "Member years should be 1 to 99", RegexOptions.None);

        regexCondition.Should().Match<RegexCondition<Customer>>(r => r.ConditionName == "RegexCondition" && r.IsLambdaPredicate == false && r.FailureMessage == "Member years should be 1 to 99"
                                                               && r.CompiledPrediate == null && r.ContextType == typeof(Customer) && r.AdditionalInfo.Count == 1
                                                               && r.AdditionalInfo[GlobalStrings.Regex_Pattern_Key] == "^[1-9]{1,2}$");
    }

}

