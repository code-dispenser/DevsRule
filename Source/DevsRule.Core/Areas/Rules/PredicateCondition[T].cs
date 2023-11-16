using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Validation;
using System.Linq.Expressions;

namespace DevsRule.Core.Common.Models;

/// <summary>
/// A condition that uses a compiled lambda predicate for its evaluation.
/// </summary>
/// <typeparam name="TContext">The data type used for the evaulation of the condition.</typeparam>
public sealed class PredicateCondition<TContext> : Condition<TContext>
{
    public PredicateCondition(string conditionName, Expression<Func<TContext, bool>> conditionExpression, string failureMessage)

        : this(conditionName, conditionExpression, failureMessage, GlobalStrings.Predicate_Condition_Evaluator){ }

    public PredicateCondition(string conditionName, Expression<Func<TContext, bool>> conditionExpression, string failureMessage, string evaluatorTypeName = GlobalStrings.Predicate_Condition_Evaluator)

    : this(conditionName, conditionExpression, failureMessage, evaluatorTypeName, new Dictionary<string, string>()) { }

    public PredicateCondition(string conditionName, Expression<Func<TContext, bool>> conditionExpression, string failureMessage, EventDetails eventDetails, string evaluatorTypeName = GlobalStrings.Predicate_Condition_Evaluator)

:       this(conditionName, conditionExpression, failureMessage, evaluatorTypeName, new Dictionary<string, string>(), eventDetails) { }

    public PredicateCondition(string conditionName, Expression<Func<TContext, bool>> conditionExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> additionalInfo, EventDetails? eventDetails = null )
    
        : base(conditionName, Check.ThrowIfNull(conditionExpression).ToString(),failureMessage, evaluatorTypeName, true, additionalInfo, eventDetails) { }
}
