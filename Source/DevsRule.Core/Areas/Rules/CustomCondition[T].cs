using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Validation;
using System.Linq.Expressions;

namespace DevsRule.Core.Areas.Rules;

/// <summary>
/// A condition that allows for either a predicate lambda or just a string to be evaluated by a custom evaluator.
/// </summary>
/// <typeparam name="TContext">The data type used for the evaluation of the condition.</typeparam>
public class CustomCondition<TContext> : Condition<TContext>
{
    public CustomCondition(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName)

    : this(conditionName, predicateExpression, failureMessage, evaluatorTypeName, new Dictionary<string, string>()) { }

    public CustomCondition(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> additionalInfo)

    : base(conditionName, Check.ThrowIfNull(predicateExpression).ToString(), failureMessage, evaluatorTypeName, true, additionalInfo) { }

    public CustomCondition(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, EventDetails eventDetails)

:       base(conditionName, Check.ThrowIfNull(predicateExpression).ToString(), failureMessage, evaluatorTypeName, true, new Dictionary<string, string>(), eventDetails) { }

    public CustomCondition(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string,string> additionalInfo, EventDetails eventDetails)

:   base(conditionName, Check.ThrowIfNull(predicateExpression).ToString(), failureMessage, evaluatorTypeName, true, additionalInfo, eventDetails) { }

    public CustomCondition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName)

    : base(conditionName, toEvaluate, failureMessage, evaluatorTypeName, false, new Dictionary<string, string>()) { }

    public CustomCondition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName, Dictionary<string,string> additionalInfo) 

        : base(conditionName, toEvaluate, failureMessage, evaluatorTypeName, false, additionalInfo) { }

    public CustomCondition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName,EventDetails eventDetails)

    : base(conditionName, toEvaluate, failureMessage, evaluatorTypeName, false, new Dictionary<string, string>(), eventDetails) { }

    public CustomCondition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName, Dictionary<string, string> additionalInfo, EventDetails eventDetails)

: base(conditionName, toEvaluate, failureMessage, evaluatorTypeName, false, additionalInfo, eventDetails) { }

}
