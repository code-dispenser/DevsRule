using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Validation;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace DevsRule.Core.Areas.Rules;

///<inheritdoc cref="ICondition"/>
/// <typeparam name="TContext">The data type used for the evaluation of the condition.</typeparam>
public class Condition<TContext> : ICondition
{
    private readonly ParsingConfig _parsingConfig = new() { AllowEqualsAndToStringMethodsOnObject = true };
    public Dictionary<string, string> AdditionalInfo { get; }//TODO immutable fix?
    public EventDetails? EventDetails { get; }

    public Type     ContextType         { get; }
    public string   ToEvaluate          { get; }
    public string   FailureMessage      { get; }
    public string   EvaluatorTypeName   { get; }
    public string   ConditionName       { get; }
    public bool     IsLambdaPredicate   { get; }

    public Func<TContext, bool>? CompiledPredicate { get; }


    internal Condition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName, bool isLambdaPredicate)

        : this(conditionName, toEvaluate, failureMessage, evaluatorTypeName, isLambdaPredicate, new Dictionary<string, string>()) { }

    internal Condition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName, bool isLambdaPredicate, Dictionary<string, string> additionalInfo)

    : this(conditionName, toEvaluate, failureMessage, evaluatorTypeName, isLambdaPredicate, additionalInfo, null) { }

    internal Condition(string conditionName, string toEvaluate, string failureMessage, string evaluatorTypeName, bool isLambdaPredicate, Dictionary<string, string> additionalInfo, EventDetails? eventDetails = null)
    {
        ConditionName       = Check.ThrowIfNullOrWhitespace(conditionName).Trim(); 
        ToEvaluate          = Check.ThrowIfNullOrWhitespace(toEvaluate);
        FailureMessage      = Check.ThrowIfNullOrWhitespace(failureMessage);
        EvaluatorTypeName   = Check.ThrowIfNullOrWhitespace(evaluatorTypeName).Trim();
        ContextType         = typeof(TContext);
        IsLambdaPredicate   = isLambdaPredicate;
        EventDetails        = eventDetails;
        AdditionalInfo      = (additionalInfo == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(additionalInfo);

        if (true == isLambdaPredicate) CompiledPredicate = BuildPredicateFromString(toEvaluate!);
    }


    /// <summary>
    /// Evaluates the condition using the condition evaluator.
    /// </summary>
    /// <param name="evaluator">An object that implements the IConditionEvaluator&al;TContext&gt; interface.</param>
    /// <param name="data">The data context to be evaluated with.</param>
    /// <param name="cancellationToken">The cancellation token used to signify any cancellation requests.</param>
    /// <param name="tenantID">Optional tenantID for multitenant scenarios, the default is "All_Tenants".</param>
    /// <returns>an EvaluationResult which contains a boolean indicating success or failure, any failure message and/or exception.</returns>
    public async Task<EvaluationResult> EvaluateWith(IConditionEvaluator<TContext> evaluator, TContext data, CancellationToken cancellationToken, string tenantID)

        => await evaluator.Evaluate(this, data, cancellationToken, tenantID).ConfigureAwait(false);

    private Func<TContext,bool> BuildPredicateFromString(string conditionExpression)
    {
        string[] expressionParts = conditionExpression.Split("=>", StringSplitOptions.TrimEntries);

        var identifier = expressionParts[0];

        ParameterExpression parameter = Expression.Parameter(typeof(TContext), identifier);

        LambdaExpression lambdaExpression = DynamicExpressionParser.ParseLambda(_parsingConfig, new[] { parameter }, typeof(bool), conditionExpression);

        return (Func<TContext, bool>)lambdaExpression.Compile();
    }

}
