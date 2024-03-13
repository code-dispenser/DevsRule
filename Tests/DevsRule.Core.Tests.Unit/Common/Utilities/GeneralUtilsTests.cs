using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Utilities;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Common.Utilities;

public class GeneralUtilsTests
{
    [Fact]

    public void The_create_dictionary_method_should_create_the_correct_entries_with_regex_none_being_excluded()
        => GeneralUtils.CreateDictionaryForRegex("pattern", RegexOptions.None)
                .Should().HaveCount(1).And.Contain(
                                                    KeyValuePair.Create(GlobalStrings.Regex_Pattern_Key, "pattern")
                                                  );
    [Fact]
    public void The_create_dictionary_method_should_create_the_correct_combination_of_entries()

    => GeneralUtils.CreateDictionaryForRegex("pattern", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline 
                                                      | RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking | RegexOptions.RightToLeft)
            .Should().Contain(
                                KeyValuePair.Create(GlobalStrings.Regex_Pattern_Key, "pattern"),
                                KeyValuePair.Create(GlobalStrings.Regex_Compiled_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_CultureInvariant_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_ECMAScript_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_ExplicitCapture_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_IgnoreCase_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_IgnorePatternWhitespace_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_Multiline_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_NonBacktracking_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_RightToLeft_Key, "true"),
                                KeyValuePair.Create(GlobalStrings.Regex_Singleline_Key, "true")
                             );

    [Fact]
    public void The_create_dictionary_method_should_add_any_additional_key_pairs_if_present()

        => GeneralUtils.CreateDictionaryForRegex("pattern", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant, ("CustomKeyOne", "One"),("CustomKeyTwo","Two"))
                .Should().Contain(
                                    KeyValuePair.Create(GlobalStrings.Regex_Pattern_Key, "pattern"),
                                    KeyValuePair.Create(GlobalStrings.Regex_CultureInvariant_Key, "true"),
                                    KeyValuePair.Create("CustomKeyOne","One"),
                                    KeyValuePair.Create("CustomKeyTwo","Two")

                                 );
}
