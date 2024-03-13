using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Evaluators;

public sealed class PredicateConditionEvaluator<TContext> : ConditionEvaluatorBase<TContext>
{
    public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID) 
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (false == condition.IsLambdaPredicate)
        {
            return await Task.FromResult(new EvaluationResult(false,String.Empty,new PredicateConditionCompilationException(GlobalStrings.Predicate_Condition_Compilation_Exception_Message)))
                                .ConfigureAwait(false);
        }
     
        var result          = condition.CompiledPredicate!(data);
        var failureMessage = result ? String.Empty : base.BuildFailureMessage(condition.FailureMessage, data!, ConditionEvaluatorBase<TContext>.MessageRegex);

        return await Task.FromResult(new EvaluationResult(result,failureMessage)).ConfigureAwait(false);
    }

}

