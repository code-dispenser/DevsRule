using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Rules
{
    /*
       * I added this interface mainly for the xml comments as to not obstruct the code in the Rule class.
       * Currently the xml comments is its only purpose
   */

    /// <summary>
    /// The Rule object is the container for the condition sets which contain the conditions that are to be evaluated.
    /// The Rule object can also holds a failure/default value for a failed rule, with an success value coming from the passing
    /// condition set. 
    /// Conditions within a condition set use short-circuit And logic between each condition, with the condition sets using 
    /// short-circuit Or logic between each set.
    /// </summary>
    public interface IRule
    {
        IReadOnlyList<IConditionSet> ConditionSets { get; }
        EventDetails?   RuleEventDetails    { get; }
        string          CultureID           { get; }
        string          FailureValue        { get; }
        bool            IsEnabled           { get; set; }
        string          RuleName            { get; }
        string          TenantID            { get; }



        /// <summary>
        /// The Evaluate method starts the evaluation process. For each condition set within the rule the ConditionSet.EvaluateConditions method is called.
        /// And for each condition within a condition set the Condition.EvaluateWith method is called.
        /// </summary>
        /// <param name="resolver">The resolver is a delegate that is used to find and create the condition evaluator per condition.
        /// Technically you could create and pass in your own resolver, more often you will just delegate this to the ConditionEngine.GetEvaluatorByName method.
        /// </param>
        /// <param name="contexts">Contains the array of DataContexts for all conditions within a rule.</param>
        /// <param name="eventPublisher">A delegate that will be used to publish events. You should use the ConditionEngine.EventPublisher.</param>
        /// <param name="cancellationToken">The cancellation token used to signify any cancellation requests, passed to all evaluators and event handlers 
        /// within scope of the respective rule.
        /// </param>
        /// <returns>A RuleResult containing all of the information about the evaluation path, failure messages, exceptions, timings and the overall 
        /// outcome and return value if specified by the rule.
        /// </returns>
        Task<RuleResult> Evaluate(ConditionEvaluatorResolver resolver, RuleData contexts, EventPublisher eventPublisher, CancellationToken cancellationToken = default);


        /// <summary>
        /// Adds a condition set to the rules collection of condition sets.
        /// </summary>
        /// <param name="conditionSet">The condition set to add.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="conditionSet"/> is null</exception>
        /// <returns>the rule for chaining.</returns>
        Rule OrConditionSet(IConditionSet conditionSet);


        /// <summary>
        /// Allows for the output of a JSON containing the rule details that can be written to file or a database, for example.
        /// </summary>
        /// <param name="writeIndented">When true will pretty print the JSON.</param>
        /// <param name="useEscaped">When true will escape certain charaters such as &lt; using UTF8 encoding.</param>
        /// <returns>a rule as a JSON formatted string.</returns>
        string ToJsonString(bool writeIndented = false, bool useEscaped = true);
    }
}