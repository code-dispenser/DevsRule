using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Rules;

/// <summary>
/// Holds a set of conditions to be evaluated.
/// </summary>
public interface IConditionSet
{
    public IReadOnlyList<ICondition> Conditions { get; }
    public string SetValue { get; }
    public string ConditionSetName { get; }


    /// <summary>
    /// The EvaluateConditions enumerates its collection of conditions and for each condition calls the Condition.EvaluateWith method.
    /// Conditions within a set short-ciruit using And logic, if the first passes it will then evaluate the next condition otherwise
    /// it will return a failing result.
    /// </summary>
    /// <param name="resolver">The resolver is a delegate that is used to find and create the condition evaluator per condition.
    /// Technically you could create and pass in your own resolver, more often you will just delegate this to the ConditionEngine.GetEvaluatorByName method.
    /// </param>
    /// <param name="contexts">Contains the array of DataContexts for all conditions within a rule.</param>
    /// <param name="eventPublisher">A delegate that will be used to publish events. You should use the ConditionEngine.EventPublisher.</param>
    /// <param name="cancellationToken">The cancellation token used to signify any cancellation requests, passed to all evaluators and event handlers 
    /// within scope of the respective rule.
    /// </param>
    /// <exception cref="MissingRuleContextsException">Thrown when <paramref name="contexts"/> is null or contains a null context.</exception>   
    /// <exception cref="MissingContditionsException">Thrwon when there are no conditions in the condition set.</exception>
    /// <returns>A ConditionResult containing all of the information about the evaluation path, failure messages, exceptions and timings.</returns>
    /// <remarks>If EvaluateConditions is called from a rule the exceptions are added to the RuleResult.</remarks>
    public Task<ConditionResult> EvaluateConditions(ConditionEvaluatorResolver resolver, RuleData contexts, EventPublisher eventPublisher, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a condition to the condition set.
    /// </summary>
    /// <param name="condition"></param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="condition"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the condition is not assignalbe from ICondition&lt;TContext&gt;.</exception>
    /// <returns>the condition set for chaining.</returns>
    ConditionSet AndCondition(dynamic condition);


    /// <summary>
    /// Allows for the removal of an added condition. 
    /// </summary>
    /// <remarks>The intention of this was in order to be able to edit a rule after cloning one</remarks>
    /// <param name="condition">The condition to remove.</param>
    void RemoveCondition(dynamic condition);


    /// <summary>
    /// Removes the condition by its index within the condition set.
    /// </summary>
    /// <param name="index">The index number.</param>
    void RemoveConditionByIndex(int index);
}

