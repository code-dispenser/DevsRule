using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Utilities;
using DevsRule.Core.Common.Validation;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace DevsRule.Core.Common.Models;

/// <summary>
/// A condition that uses a regular expression for its evaluation. The ToEvaluate property holds the property path for the property to be evaluated.
/// Various regex options can be set which are passed via the underlying conditions additionalinfo Dictionary&lt;string,string&gt; property.
/// </summary>
/// <typeparam name="TContext">The data type used for the evaulation of the condition.</typeparam>
public sealed class RegexCondition<TContext> : Condition<TContext>
{
    /// <summary>
    /// A helper method to help create a condition for evaluation against a regular expression.
    /// </summary>
    /// <param name="conditionName">The name of the condition.</param>
    /// <param name="propertyExpression">An expression that allows strong typing of the property name, path.</param>
    /// <param name="pattern">The pattern for the reqular expression.</param>
    /// <param name="failureMessage">The message used if the pattern does not match the properties data.</param>
    /// <param name="regexOptions">Options used for the .Net Regex class. These get added to the underlying Dictionary&lt;string,string&gt;</param>
    /// <param name="additionalInfo">An array of key value pairs that will get added to the underlying conditions Dictionary&lt;string,string&gt; used to pass addtional information to a custom evaluator.</param>
    /// <returns>the RegexCondition in order for chaining.</returns>
    public static RegexCondition<TContext> Create(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, params (string key, string value)[] additionalInfo)
    {
        Dictionary<string, string> requiredInfo = GeneralUtils.CreateDictionaryForRegex(pattern, regexOptions, additionalInfo);

        return new RegexCondition<TContext>(conditionName, propertyExpression, failureMessage, requiredInfo);
    }

    /// <summary>
    /// A helper method to help create a condition for evaluation against a regular expression.
    /// </summary>
    /// <param name="conditionName">The name of the condition.</param>
    /// <param name="propertyExpression">An expression that allows strong typing of the property name, path.</param>
    /// <param name="pattern">The pattern for the reqular expression.</param>
    /// <param name="failureMessage">The message used if the pattern does not match the properties data.</param>
    /// <param name="regexOptions">Options used for the .Net Regex class. These get added to the underlying Dictionary&lt;string,string&gt;</param>
    /// <param name="eventDetails">A optional EventDetails object used to hold the eventing details.</param>
    /// <param name="additionalInfo">An array of key value pairs that will get added to the underlying conditions Dictionary&lt;string,string&gt; used to pass addtional information to a custom evaluator.</param>
    /// <returns>the RegexCondition in order for chaining.</returns>
    public static RegexCondition<TContext> Create(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails eventDetails, params (string key, string value)[] additionalInfo)
    {
        Dictionary<string, string> requiredInfo = GeneralUtils.CreateDictionaryForRegex(pattern, regexOptions, additionalInfo);

        return new RegexCondition<TContext>(conditionName, propertyExpression, failureMessage, requiredInfo, eventDetails);
    }


    public RegexCondition(string conditionName, Expression<Func<TContext, object>> propertyExpression, string failureMessage, Dictionary<string, string> additionalInfo, EventDetails? eventDetails = null)

        : base(conditionName, GetPropertyNameFromExpressionString(propertyExpression), failureMessage, GlobalStrings.Regex_Condition_Evaluator, false, CheckPatternIsInDictionary(additionalInfo, conditionName),eventDetails) {}
   

    private static Dictionary<string, string> CheckPatternIsInDictionary(Dictionary<string, string> additionalInfo, string conditionName)
    {
        if (additionalInfo != null && additionalInfo.ContainsKey(GlobalStrings.Regex_Pattern_Key) && false == String.IsNullOrWhiteSpace(additionalInfo[GlobalStrings.Regex_Pattern_Key])) return additionalInfo;

        throw new MissingRegexPatternException(String.Format(GlobalStrings.MIssing_Regex_Pattern_Or_Pattern_Empty_Exception_Message, conditionName));
    }

    private static string GetPropertyNameFromExpressionString(Expression expression)
    {
        var expressionString = Check.ThrowIfNull(expression).ToString();

        var nameParts    = expressionString.Split("=>",StringSplitOptions.TrimEntries);
        var identifier   = string.Concat(nameParts[0],".");
        var propertyPath = nameParts[1].Remove(0,identifier.Length);

        return propertyPath;
    }

}
