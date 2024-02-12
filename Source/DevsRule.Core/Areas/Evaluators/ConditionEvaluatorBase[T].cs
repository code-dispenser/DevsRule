using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Validation;
using System.Text.RegularExpressions;

namespace DevsRule.Core.Areas.Evaluators;

/// <summary>
/// Base class used for condition evaluators. The base class has helper methods to build failure messages if tokens are present within the in the 
/// conditions failure message.
/// </summary>
/// <typeparam name="TContext">The data type used for the evaluation of the condition</typeparam>
public abstract class ConditionEvaluatorBase<TContext> : IConditionEvaluator<TContext>
{
    protected static Regex MessageRegex { get; } = new Regex("@{.*?}", RegexOptions.Compiled);


    /// <summary>
    /// Used to evaluate the condition.
    /// </summary>
    /// <param name="condition">The condition to be evaluated.</param>
    /// <param name="data">The data for the evaluation.</param>
    /// <param name="cancellationToken">The cancellation token used to signify any cancellation requests.<returns></returns>
    public abstract Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID);


    /// <summary>
    /// Gets the property value as a streing from the data context.
    /// </summary>
    /// <param name="context">The data context used in the condition.</param>
    /// <param name="propertyPath">The property path to the data on the data context.</param>
    /// <param name="replaceNullWith">Optional string value used when properties are not found. If left blank any unmatched 
    /// property paths will use "N/A".</param>
    /// <returns></returns>
    protected virtual string GetPropertValueAsString(object context, string propertyPath, string replaceNullWith = "N/A")
    {
        propertyPath = Check.ThrowIfNull(propertyPath);
        context = Check.ThrowIfNull(context);

        object? objectValue = context;
        object? propertyValue = null;

        foreach (var propertyName in propertyPath.Split(".", StringSplitOptions.TrimEntries))
        {
            var propertyInfo = objectValue.GetType().GetProperty(propertyName);

            if (propertyInfo == null) break;

            objectValue = propertyInfo.GetValue(objectValue, null);
            propertyValue = objectValue;

            if (objectValue == null) break;

        }

        return propertyValue?.ToString() ?? replaceNullWith;
    }

    /// <summary>
    /// Method used to build up a failure message replacing any peropery tokens with the property value.
    /// </summary>
    /// <param name="failureMessage">The failure message, predominately this will be from the condition.</param>
    /// <param name="contextData">The data used in the condition.</param>
    /// <param name="matcher">A regex for matching the replacement pattern. The condition base has a compiled MessageRegex for this purpose.</param>
    /// <param name="missingPropertyText">Optional string value used when properties are not found. If left blank any unmatched 
    /// property paths will use "N/A".</param>
    /// <returns></returns>
    protected virtual string BuildFailureMessage(string failureMessage, object contextData, Regex matcher, string missingPropertyText = "N/A")
    {
        if (string.IsNullOrWhiteSpace(failureMessage)) return failureMessage;

        var matches = matcher.Matches(failureMessage);

        foreach (Match match in matches)//TODO maybe optimize by using span and slicing?
        {
            var propertyPath = match.Value.Substring(2, match.Value.Length - 3);
            var replacementValue = GetPropertValueAsString(contextData, propertyPath, missingPropertyText);

            failureMessage = Regex.Replace(failureMessage, match.Value, replacementValue);
        }

        return failureMessage;
    }


}
