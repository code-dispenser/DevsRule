using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Evaluators;

public interface IConditionEvaluator { }

public interface IConditionEvaluator<TContext> : IConditionEvaluator
{
    public Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID = GlobalStrings.Default_TenantID);
}