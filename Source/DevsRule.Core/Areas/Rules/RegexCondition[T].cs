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
/// <typeparam name="TContext">The data type used for the evaluation of the condition.</typeparam>
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
    /// <param name="eventDetails">An optional EventDetails object used to hold the eventing details.</param>
    /// <returns>the RegexCondition in order for chaining.</returns>
    public static RegexCondition<TContext> Create(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails? eventDetails= null)
    {
        Dictionary<string, string> requiredInfo = GeneralUtils.CreateDictionaryForRegex(pattern, regexOptions);

        return new RegexCondition<TContext>(conditionName, propertyExpression, failureMessage, requiredInfo, eventDetails, GlobalStrings.Regex_Condition_Evaluator);
    }

    /// <summary>
    /// A helper method to help create a condition for evaluation against a regular expression.
    /// </summary>
    /// <param name="conditionName">The name of the condition.</param>
    /// <param name="propertyExpression">An expression that allows strong typing of the property name, path.</param>
    /// <param name="pattern">The pattern for the reqular expression.</param>
    /// <param name="failureMessage">The message used if the pattern does not match the properties data.</param>
    /// <param name="regexOptions">Options used for the .Net Regex class. These get added to the underlying Dictionary&lt;string,string&gt;</param>
    /// <param name="evaluatorTypeName">The name of the customer evaluator for the RegexCondition</param>
    /// <param name="eventDetails">An optional EventDetails object used to hold the eventing details, use null if not needed.</param>
    /// <param name="additionalInfo">An array of key value pairs that will get added to the underlying conditions Dictionary&lt;string,string&gt; used to pass addtional information to a custom evaluator.</param>
    /// <returns>the RegexCondition in order for chaining.</returns>
    public static RegexCondition<TContext> Create(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, string evaluatorTypeName, EventDetails? eventDetails, params (string key, string value)[] additionalInfo)
    {
        Dictionary<string, string> requiredInfo = GeneralUtils.CreateDictionaryForRegex(pattern, regexOptions, additionalInfo);

        return new RegexCondition<TContext>(conditionName, propertyExpression, failureMessage, requiredInfo, eventDetails, GlobalStrings.Regex_Condition_Evaluator);
    }

    public RegexCondition(string conditionName, Expression<Func<TContext, object>> propertyExpression, string failureMessage, Dictionary<string, string> additionalInfo)

    : base(conditionName, GetPropertyPathFromExpressionString(propertyExpression), failureMessage, GlobalStrings.Regex_Condition_Evaluator, false, CheckPatternIsInDictionary(additionalInfo, conditionName), null) { }

    public RegexCondition(string conditionName, Expression<Func<TContext, object>> propertyExpression, string failureMessage, Dictionary<string, string> additionalInfo, EventDetails? eventDetails = null)

        : base(conditionName, GetPropertyPathFromExpressionString(propertyExpression), failureMessage, GlobalStrings.Regex_Condition_Evaluator, false, CheckPatternIsInDictionary(additionalInfo, conditionName),eventDetails) {}

    public RegexCondition(string conditionName, Expression<Func<TContext, object>> propertyExpression, string failureMessage, Dictionary<string, string> additionalInfo, EventDetails? eventDetails = null, string evaluatorTypeName = GlobalStrings.Regex_Condition_Evaluator)

    : base(conditionName, GetPropertyPathFromExpressionString(propertyExpression), failureMessage, GlobalStrings.Regex_Condition_Evaluator, false, CheckPatternIsInDictionary(additionalInfo, conditionName), eventDetails) { }

    private static Dictionary<string, string> CheckPatternIsInDictionary(Dictionary<string, string> additionalInfo, string conditionName)
    {
        if (additionalInfo != null && additionalInfo.ContainsKey(GlobalStrings.Regex_Pattern_Key) && false == String.IsNullOrWhiteSpace(additionalInfo[GlobalStrings.Regex_Pattern_Key])) return additionalInfo;

        throw new MissingRegexPatternException(String.Format(GlobalStrings.Missing_Regex_Pattern_Or_Pattern_Empty_Exception_Message, conditionName));
    }

    private static string GetPropertyPathFromExpressionString(Expression<Func<TContext,object>> expression)
    {
        var expressionString = Check.ThrowIfNull(expression).ToString();

        MemberExpression memberExpression;

        if (expression.Body is UnaryExpression unaryExpression)
        {
            memberExpression = (MemberExpression)unaryExpression.Operand;
        }
        else
        {
            memberExpression = (MemberExpression)expression.Body;
        }

        var propertyPath = GetMemberAccessPath(memberExpression);
        
        return propertyPath;
    }

    private static string GetMemberAccessPath(MemberExpression memberExpression)
    {
        if (memberExpression.Expression is MemberExpression innerMemberExpression)
        {
            return GetMemberAccessPath(innerMemberExpression) + "." + memberExpression.Member.Name;
        }

        return memberExpression.Member.Name;
    }
}
